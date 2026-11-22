using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
using WebGLFile = FrostweepGames.Plugins.WebGLFileBrowser.File;
using WebGLFileInfo = FrostweepGames.Plugins.WebGLFileBrowser.FileInfo;
using Debug = UnityEngine.Debug;

/// <summary>
/// Uploads real models the way the upload dialog does (<see cref="ModelUploadManager"/>): the picked files go to
/// <see cref="AssetManager.PrepareAssetFromFiles"/>, a frame passes, then <see cref="AssetManager.BuildPreparedAsset"/>.
/// Every model gets screenshots and stats in TestResults/ModelUpload/&lt;run&gt;/report.html
/// (menu Tools > Model Upload Tests > Open Latest Report).
/// </summary>
/// <remarks>
/// Play mode, because the managers are MonoBehaviour singletons set up in Awake and the loaded model is rendered.
/// With the WebGL build target active the editor loads models synchronously, like the browser does.
/// </remarks>
public class ModelUploadTests {

    // Our biggest real model (Ústí–Bílina) is about 3 km across. Much bigger means broken numbers, e.g. decimal commas
    const float MaxPlausibleSizeMeters = 50000f;

    const string FileLoadingManagerPrefabPath = "Assets/_Features/Utilities/AssetManagement/ModelAssets/FileLoadingManager/FileLoadingManager.prefab";

    GameObject _managersRoot;
    ModelAsset _asset;
    string _createdHashFolder;
    readonly List<string> _warnings = new List<string>();
    readonly List<string> _errors = new List<string>();

    public static IEnumerable<TestModelCatalog.ModelCase> StandardCases() => TestModelCatalog.Standard();
    public static IEnumerable<TestModelCatalog.ModelCase> HeavyCases() => TestModelCatalog.Heavy();

    [OneTimeSetUp]
    public void OneTimeSetUp() => ModelUploadReport.BeginRun();

    [OneTimeTearDown]
    public void OneTimeTearDown() => Debug.Log($"[ModelUploadTests] Report: {Path.Combine(ModelUploadReport.RunFolder, "report.html")}");

    [SetUp]
    public void SetUp() {
        _managersRoot = new GameObject("ModelUploadTests Managers");

#if UNITY_EDITOR
        // The prefab holds the allowed extension lists the app uses
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(FileLoadingManagerPrefabPath);
        Assert.IsNotNull(prefab, $"Missing {FileLoadingManagerPrefabPath}");
        UnityEngine.Object.Instantiate(prefab, _managersRoot.transform);
#else
        Assert.Ignore("The upload tests run in the editor only.");
#endif

        var container = new GameObject("AssetContainer");
        container.transform.SetParent(_managersRoot.transform);
        var assetManager = new GameObject("AssetManager").AddComponent<AssetManager>();
        assetManager.transform.SetParent(_managersRoot.transform);
        assetManager.AssetContainer = container;

        Assert.AreSame(assetManager, AssetManager.Instance, "AssetManager singleton wasn't replaced");
        Assert.IsNotNull(FileLoadingManager.Instance, "FileLoadingManager singleton missing");

        _warnings.Clear();
        _errors.Clear();
        Application.logMessageReceived += OnLog;
    }

    [UnityTearDown]
    public IEnumerator TearDown() {
        Application.logMessageReceived -= OnLog;

        if (_asset != null) UnityEngine.Object.DestroyImmediate(_asset.gameObject);
        _asset = null;
        if (_managersRoot != null) UnityEngine.Object.DestroyImmediate(_managersRoot);

        // Delete the copy in persistentDataPath only if this test made it (it may be a real cached upload otherwise)
        if (_createdHashFolder != null && Directory.Exists(_createdHashFolder)) {
            try { Directory.Delete(_createdHashFolder, true); } catch (Exception e) { Debug.LogWarning($"Couldn't delete {_createdHashFolder}: {e.Message}"); }
        }
        _createdHashFolder = null;

        // Free the meshes and textures before the next model
        yield return Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    void OnLog(string message, string stackTrace, LogType type) {
        if (type == LogType.Warning) {
            // FileLoadingManager creates new loader options for every model, TriLib warns about it every time
            if (message.StartsWith("TriLib: You are creating a new AssetLoaderOptions", StringComparison.Ordinal)) return;
            _warnings.Add(message);
        }
        else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) _errors.Add(message);
    }

    [UnityTest, Timeout(300000)]
    public IEnumerator UploadModel([ValueSource(nameof(StandardCases))] TestModelCatalog.ModelCase modelCase) {
        yield return Upload(modelCase);
    }

    [UnityTest, Explicit("Big models, run them on purpose"), Category("Heavy"), Timeout(1200000)]
    public IEnumerator UploadHeavyModel([ValueSource(nameof(HeavyCases))] TestModelCatalog.ModelCase modelCase) {
        yield return Upload(modelCase);
    }

    IEnumerator Upload(TestModelCatalog.ModelCase modelCase) {
        if (!modelCase.MainFileExists) {
            Assert.Ignore($"Model not on this machine: {Path.Combine(modelCase.Root, modelCase.FilePatterns[0])} " +
                "(set the folders with Tools > Model Upload Tests > Set Model Folders)");
        }

        List<string> paths = modelCase.ResolveFiles();
        var entry = new ModelUploadReport.Entry {
            Name = modelCase.Name,
            Notes = modelCase.Notes,
            InputFiles = paths.Select(p => p.Substring(modelCase.Root.Length).TrimStart('\\', '/')).ToList(),
        };

        // Same objects the file picker hands over
        WebGLFile[] files = paths.Select(ToWebGLFile).ToArray();
        entry.InputMB = files.Sum(f => (double)f.data.Length) / (1024 * 1024);

        string hashFolder = Path.Combine(Application.persistentDataPath, Md5(files[0].data));
        if (!Directory.Exists(hashFolder)) _createdHashFolder = hashFolder;

        GC.Collect();
        long memoryBefore = Profiler.GetTotalAllocatedMemoryLong();

        var watch = Stopwatch.StartNew();
        // PrepareAssetFromFiles / BuildPreparedAsset only exist since "Free model file bytes before TriLib parses the model".
        // Called by reflection so the tests also compile and run on older commits, which upload in one step
        MethodInfo prepareMethod = typeof(AssetManager).GetMethod("PrepareAssetFromFiles");
        MethodInfo buildMethod = typeof(AssetManager).GetMethod("BuildPreparedAsset");
        bool twoSteps = prepareMethod != null && buildMethod != null;

        object prepared = null;
        if (twoSteps) {
            try {
                prepared = prepareMethod.Invoke(AssetManager.Instance, new object[] { files });
            } catch (Exception e) {
                entry.Failure = "PrepareAssetFromFiles threw: " + Unwrap(e).Message;
                Debug.LogException(Unwrap(e));
            }
            entry.PrepareSeconds = watch.Elapsed.TotalSeconds;

            // Like ModelUploadManager: the bytes are released and a frame passes before the parse
            files = null;
            yield return null;

            BeginBuildMemory();
            watch.Restart();
            try {
                if (prepared != null) _asset = (ModelAsset)buildMethod.Invoke(AssetManager.Instance, new object[] { prepared, null });
            } catch (Exception e) {
                entry.Failure = "BuildPreparedAsset threw: " + Unwrap(e).Message;
                Debug.LogException(Unwrap(e));
            }
        } else {
            entry.Notes += " [one-step upload: CreateNewAssetFromFiles]";
            BeginBuildMemory();
            try {
                _asset = AssetManager.Instance.CreateNewAssetFromFiles(files);
            } catch (Exception e) {
                entry.Failure = "CreateNewAssetFromFiles threw: " + e.Message;
                Debug.LogException(e);
            }
            files = null;
        }
        entry.BuildSeconds = watch.Elapsed.TotalSeconds;
        entry.MemoryDeltaMB = (Profiler.GetTotalAllocatedMemoryLong() - memoryBefore) / (1024.0 * 1024.0);
        entry.NativeRetainedMB = (Profiler.GetTotalAllocatedMemoryLong() - _nativeBeforeBuild) / MB;

        // The build ran within one frame. On WebGL the garbage collector only runs between frames, so all C# memory
        // allocated during it is in the heap at once at the end of the parse: that's the upload's peak
        yield return null;
        entry.ParseAllocatedMB = _allocatedInFrame.Valid ? _allocatedInFrame.LastValue / MB : -1;
        _allocatedInFrame.Dispose();
        GC.Collect();
        entry.ManagedRetainedMB = (Profiler.GetMonoUsedSizeLong() - _managedBeforeBuild) / MB;

        entry.Loaded = _asset != null;
        if (!entry.Loaded && entry.Failure == null)
            entry.Failure = twoSteps && prepared == null ? "No main model in the selection." : "The model didn't load (the upload returned null).";

        Bounds bounds = default;
        bool hasBounds = false;
        if (_asset != null) {
            CollectStats(_asset.gameObject, entry);
            hasBounds = FileLoadingManager.TryCalculateBounds(_asset.gameObject, out bounds);
            entry.BoundsSize = hasBounds ? bounds.size : Vector3.zero;

            _asset.gameObject.SetActive(true);
            try {
                ModelUploadReport.TakeScreenshots(_asset.gameObject, entry);
            } catch (Exception e) {
                Debug.LogWarning($"Screenshot failed: {e.Message}");
            }
            _asset.gameObject.SetActive(false);
        }

        entry.Warnings.AddRange(_warnings);
        entry.Errors.AddRange(_errors);
        ModelUploadReport.Add(entry);

        // Checks. Logged errors fail the test too (Unity Test Framework does that on its own)
        Assert.IsTrue(entry.Loaded, entry.Failure);
        var problems = new List<string>();
        if (entry.Renderers == 0) problems.Add("no renderers");
        if (entry.Vertices == 0) problems.Add("no vertices");
        if (!hasBounds) problems.Add("no bounds");
        else if (!IsFinite(bounds.size) || !IsFinite(bounds.center)) problems.Add($"bounds not finite ({bounds})");
        else if (bounds.size.sqrMagnitude <= 0f) problems.Add("bounds have zero size");
        else if (MaxComponent(bounds.size) > MaxPlausibleSizeMeters)
            problems.Add($"model is {MaxComponent(bounds.size) / 1000f:N0} km across, the coordinates were probably misread (decimal commas?)");
        if (modelCase.ExpectTextures && entry.Textures == 0) problems.Add("no textures on materials, but the selection has some");

        if (problems.Count > 0) Assert.Fail($"{modelCase.Name}: {string.Join("; ", problems)}");
    }

    static void CollectStats(GameObject model, ModelUploadReport.Entry entry) {
        var renderers = model.GetComponentsInChildren<Renderer>(true);
        var meshes = new HashSet<Mesh>();
        var materials = new HashSet<Material>();
        var textures = new HashSet<Texture>();

        foreach (var r in renderers) {
            Mesh mesh = null;
            if (r is SkinnedMeshRenderer smr) mesh = smr.sharedMesh;
            else if (r.TryGetComponent(out MeshFilter mf)) mesh = mf.sharedMesh;
            if (mesh != null) meshes.Add(mesh);
            foreach (var m in r.sharedMaterials) if (m != null) materials.Add(m);
        }

        foreach (var mesh in meshes) {
            entry.Vertices += mesh.vertexCount;
            entry.SubMeshes += mesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++) {
                if (mesh.GetTopology(i) == MeshTopology.Triangles) entry.Triangles += (long)mesh.GetIndexCount(i) / 3;
            }
        }

        foreach (var m in materials) {
            if (m.HasProperty("_MainTex") && IsModelTexture(m.mainTexture)) entry.MaterialsWithMainTexture++;
            foreach (int id in m.GetTexturePropertyNameIDs()) {
                Texture t = m.GetTexture(id);
                if (IsModelTexture(t)) textures.Add(t);
            }
        }

        entry.Renderers = renderers.Length;
        entry.Meshes = meshes.Count;
        entry.Materials = materials.Count;
        entry.Textures = textures.Count;
        entry.TextureNames = textures.Select(t => $"{t.name} ({t.width}×{t.height})").OrderBy(n => n).ToList();
    }

    // TriLib fills empty material slots with its own 16x16 placeholders (TriLibWhite, TriLibBlack, TriLibBump, ...)
    static bool IsModelTexture(Texture t) => t != null && !t.name.StartsWith("TriLib", StringComparison.Ordinal);

    static WebGLFile ToWebGLFile(string path) {
        // Same as FileBrowserManager.ShowEditorLoadDialog
        byte[] data = File.ReadAllBytes(path);
        return new WebGLFile {
            fileInfo = new WebGLFileInfo {
                extension = Path.GetExtension(path),
                name = Path.GetFileNameWithoutExtension(path),
                fullName = Path.GetFileName(path),
                path = path,
                length = data.Length,
                size = data.Length
            },
            data = data
        };
    }

    // Same hash as AssetManager.GetFileHash, to know which persistentDataPath folder the upload writes to
    static string Md5(byte[] data) {
        using (var md5 = MD5.Create()) return BitConverter.ToString(md5.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
    }

    const double MB = 1024.0 * 1024.0;
    long _nativeBeforeBuild;
    long _managedBeforeBuild;
    Unity.Profiling.ProfilerRecorder _allocatedInFrame;

    // Right before the model is built: the file bytes are already released (or still held, on older commits)
    void BeginBuildMemory() {
        GC.Collect();
        _managedBeforeBuild = Profiler.GetMonoUsedSizeLong();
        _nativeBeforeBuild = Profiler.GetTotalAllocatedMemoryLong();
        _allocatedInFrame = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory, "GC Allocated In Frame");
    }

    static Exception Unwrap(Exception e) => e is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : e;

    static float MaxComponent(Vector3 v) => Mathf.Max(v.x, Mathf.Max(v.y, v.z));

    static bool IsFinite(Vector3 v) => !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                                          float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
}
