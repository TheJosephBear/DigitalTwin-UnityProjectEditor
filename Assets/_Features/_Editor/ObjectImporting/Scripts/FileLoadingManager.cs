using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using UnityEngine;
using FrostweepGames.Plugins.WebGLFileBrowser;
using System.Linq;
using TriLibCore;
using UnityEditor.PackageManager;

/// <summary>
/// Responsible for creating GameObjects from files from any source.
/// </summary>
/// <remarks>
/// For creating GameObjects that will be part of the Digital twin project, use <see cref="AssetManager"/> instead.
/// </remarks>
public class FileLoadingManager : Singleton<FileLoadingManager> {

    [Header("Allowed Extensions")]
    [Tooltip("All allowed extensions (Models, Materials, Textures, Archives). E.g., 'fbx', 'obj', 'png', 'zip'")]
    [SerializeField] private List<string> _allowedFileExtensions = new List<string>();

    [Tooltip("Main 3D model file extensions used for hashing and core asset identity. E.g., 'fbx', 'obj', 'glb', 'gltf'")]
    [SerializeField] private List<string> _allowedFileExtensionsMain = new List<string>();

    #region Public interface for uploading from PC

    /// <summary>
    /// Upload that uses path information.
    /// Works only in editor and PC build.
    /// </summary>
    /// <param name="folderOrModelPath">Path to a model file or folder containing asset files.</param>
    /// <param name="fileHash">Unique hash identifying this asset bundle.</param>
    /// <returns>Loaded model GameObject.</returns>
    public GameObject UploadFromPC(
        string folderOrModelPath,
        string fileHash
    ) {
        ResolveOriginalPaths(folderOrModelPath, out string modelPath, out string folderPath);

        if (string.IsNullOrEmpty(modelPath)) {
            Debug.LogError($"[FileLoadingManager] No main 3D model file found at path: {folderOrModelPath}");
            return null;
        }

        string copiedModelPath = CopyAssetBundleToPersistent(modelPath, folderPath, fileHash);

        return BuildModelFromCopiedFiles(copiedModelPath);
    }

    /// <summary>
    /// Uploads a single file selected via WebGL file picker.
    /// </summary>
    /// <param name="file">Selected WebGL file object.</param>
    /// <param name="fileHash">Unique hash identifying this asset.</param>
    /// <returns>Loaded model GameObject.</returns>
    public GameObject UploadFromWebGLFile(
        FrostweepGames.Plugins.WebGLFileBrowser.File file,
        string fileHash
    ) {
        if (file == null || file.fileInfo == null) {
            Debug.LogError("[FileLoadingManager] Null file passed to WebGL upload.");
            return null;
        }

        string targetRoot = GetPersistentAssetPath(fileHash);
        string fileName = Path.GetFileName(file.fileInfo.fullName);
        string targetPath = Path.Combine(targetRoot, fileName);

        // Save binary payload to persistent storage
        System.IO.File.WriteAllBytes(targetPath, file.data);

        string ext = file.fileInfo.extension;

        // Verify if the single uploaded file is a valid primary 3D model format
        if (!IsMainModelExtension(ext) && !IsAllowedExtension(ext)) {
            Debug.LogError($"[FileLoadingManager] Uploaded file extension '{ext}' is not an allowed format.");
            return null;
        }

        return BuildModelFromCopiedFiles(targetPath);
    }

    /// <summary>
    /// Uploads a multi-file selection (Model + Materials + Textures) via WebGL file picker.
    /// </summary>
    /// <param name="files">Array of selected WebGL file objects.</param>
    /// <param name="fileHash">Unique hash identifying this asset bundle.</param>
    /// <returns>Loaded model GameObject.</returns>
    public GameObject UploadFromWebGLFiles(
        FrostweepGames.Plugins.WebGLFileBrowser.File[] files,
        string fileHash
    ) {
        if (files == null || files.Length == 0) {
            Debug.LogError("[FileLoadingManager] No files provided in array.");
            return null;
        }

        string targetRoot = GetPersistentAssetPath(fileHash);
        string mainModelPath = null;

        foreach (var file in files) {
            if (file == null || file.fileInfo == null) continue;

            string fileName = Path.GetFileName(file.fileInfo.fullName);
            string targetPath = Path.Combine(targetRoot, fileName);

            // Save each file (main model, MTLs, textures, etc.) into the hash folder
            System.IO.File.WriteAllBytes(targetPath, file.data);

            string ext = file.fileInfo.extension;

            // Identify the primary 3D model file in the bundle
            if (mainModelPath == null && IsMainModelExtension(ext)) {
                mainModelPath = targetPath;
            }
        }

        // Fallback check: If no explicit main extension matched, check for any allowed extension
        if (mainModelPath == null) {
            foreach (var file in files) {
                if (file == null || file.fileInfo == null) continue;
                string ext = file.fileInfo.extension;

                if (IsAllowedExtension(ext)) {
                    mainModelPath = Path.Combine(targetRoot, Path.GetFileName(file.fileInfo.fullName));
                    break;
                }
            }
        }

        if (mainModelPath == null) {
            Debug.LogError("[FileLoadingManager] No valid primary 3D model file found in uploaded selection.");
            return null;
        }

        return BuildModelFromCopiedFiles(mainModelPath);
    }

    #endregion

    #region Public interface for downloading from server

    /// <summary>
    /// Creates an OBJ file from downloaded byte data and stores it
    /// in the asset's persistent directory.
    /// </summary>
    public void CreateOBJFromBytes(string assetHash, string fileName, byte[] data) {
        WriteFile(assetHash, fileName, data);
    }

    /// <summary>
    /// Creates an MTL file from downloaded byte data and stores it
    /// in the asset's persistent directory.
    /// </summary>
    public void CreateMTLFromBytes(string assetHash, string fileName, byte[] data) {
        WriteFile(assetHash, fileName, data);
    }

    /// <summary>
    /// Creates a texture file (PNG/JPG) from downloaded byte data and stores it
    /// in the asset's persistent directory.
    /// Multiple textures can be added for the same asset.
    /// </summary>
    public void CreateTextureFromBytes(string assetHash, string fileName, byte[] data) {
        WriteFile(assetHash, fileName, data);
    }

    /// <summary>
    /// Builds a model from files that were previously downloaded and stored in the persistent directory.
    /// All required files (main model, materials, textures) must already exist in the asset folder.
    /// </summary>
    /// <param name="assetHash">Unique hash identifying this model asset.</param>
    /// <returns>Instantiated model GameObject, deactivated by default.</returns>
    public GameObject BuildFromDownloadedFiles(string assetHash) {
        string root = GetPersistentAssetPath(assetHash);

        if (!Directory.Exists(root)) {
            Debug.LogError($"[FileLoadingManager] Asset folder does not exist: {root}");
            return null;
        }

        string[] allFiles = Directory.GetFiles(root);
        string mainModelPath = null;

        // Search for a main 3D model file matching your allowed main extensions
        foreach (string filePath in allFiles) {
            string ext = Path.GetExtension(filePath);
            if (IsMainModelExtension(ext)) {
                mainModelPath = filePath;
                break;
            }
        }

        // Fallback: If no main model extension match was found, check for any allowed extension
        if (mainModelPath == null) {
            foreach (string filePath in allFiles) {
                string ext = Path.GetExtension(filePath);
                if (IsAllowedExtension(ext)) {
                    mainModelPath = filePath;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(mainModelPath)) {
            Debug.LogError($"[FileLoadingManager] No valid 3D model file found in asset folder: {root}");
            return null;
        }

        return BuildModelFromCopiedFiles(mainModelPath);
    }

    #endregion

    #region Object loading from PC helpers

    private void ResolveOriginalPaths(string path, out string modelPath, out string folderPath) {
        if (System.IO.File.Exists(path)) {
            modelPath = path;
            folderPath = Path.GetDirectoryName(path);
        } else if (Directory.Exists(path)) {
            folderPath = path;
            modelPath = null;

            // Search directory for a primary model matching allowed main model extensions
            foreach (string file in Directory.GetFiles(folderPath)) {
                string ext = Path.GetExtension(file);
                if (IsMainModelExtension(ext)) {
                    modelPath = file;
                    break;
                }
            }

            if (modelPath == null)
                throw new FileNotFoundException($"No valid 3D model found in directory: {folderPath}");
        } else {
            throw new DirectoryNotFoundException($"Path not found: {path}");
        }
    }

    private string CopyAssetBundleToPersistent(string modelPath, string sourceFolder, string fileHash) {
        string targetRoot = GetPersistentAssetPath(fileHash);
        Directory.CreateDirectory(targetRoot);

        // Copy all allowed files (main model, MTL, textures, BIN, etc.) to the target persistent directory
        foreach (string file in Directory.GetFiles(sourceFolder)) {
            string ext = Path.GetExtension(file);
            if (IsAllowedExtension(ext)) {
                string targetPath = Path.Combine(targetRoot, Path.GetFileName(file));
                System.IO.File.Copy(file, targetPath, true);
            }
        }

        return Path.Combine(targetRoot, Path.GetFileName(modelPath));
    }

    #endregion

    #region Object building (for when you have all the files uploaded to unity)
    /// <summary>
    /// Loads any supported 3D model format (FBX, GLTF, OBJ, STL, etc.) using TriLib 2.
    /// TriLib automatically locates and binds companion files (MTL, PNG, JPG, BIN) in the same directory.
    /// </summary>
    private GameObject BuildModelFromCopiedFiles(string copiedModelPath) {
        if (!System.IO.File.Exists(copiedModelPath)) {
            Debug.LogError($"[FileLoadingManager] Model file does not exist: {copiedModelPath}");
            return null;
        }

        AssetLoaderOptions options = AssetLoader.CreateDefaultLoaderOptions();

        // Load model synchronously using TriLib 2
        AssetLoaderContext context = AssetLoader.LoadModelFromFile(copiedModelPath, null, null, null, null, null, options);

        if (context == null || context.RootGameObject == null) {
            Debug.LogError($"[FileLoadingManager] TriLib failed to load model from path: {copiedModelPath}");
            return null;
        }

        GameObject loadedModel = context.RootGameObject;
        loadedModel.SetActive(false);

        return loadedModel;
    }

    #endregion

    #region Allowed File Extensions & Filtering
    /// <summary>
    /// Checks whether the provided file extension is permitted (includes models, materials, textures, etc.).
    /// Case-insensitive and handles leading dots automatically.
    /// </summary>
    /// <param name="extension">File extension (e.g., ".obj" or "obj").</param>
    /// <returns>True if allowed; otherwise, false.</returns>
    public bool IsAllowedExtension(string extension) {
        if (string.IsNullOrEmpty(extension)) return false;
        string cleanExt = NormalizeExtension(extension);

        return _allowedFileExtensions.Exists(e => NormalizeExtension(e) == cleanExt);
    }

    /// <summary>
    /// Checks whether the provided file extension belongs to a main 3D model file format.
    /// Useful for identifying target root assets and performing MD5 hashing.
    /// </summary>
    /// <param name="extension">File extension (e.g., ".fbx" or "fbx").</param>
    /// <returns>True if it is a main 3D model extension; otherwise, false.</returns>
    public bool IsMainModelExtension(string extension) {
        if (string.IsNullOrEmpty(extension)) return false;
        string cleanExt = NormalizeExtension(extension);

        return _allowedFileExtensionsMain.Exists(e => NormalizeExtension(e) == cleanExt);
    }

    /// <summary>
    /// Returns a comma-separated string of all allowed extensions.
    /// Useful for displaying supported formats in UI or dialog prompts.
    /// </summary>
    /// <returns>Formatted string (e.g., "obj, fbx, glb, png").</returns>
    public string GetAllowedExtensionsString() {
        return string.Join(", ", _allowedFileExtensions);
    }

    /// <summary>
    /// Returns a comma-separated string of only the main 3D model extensions.
    /// </summary>
    /// <returns>Formatted string (e.g., "obj, fbx, glb, gltf").</returns>
    public string GetMainAllowedExtensionsString() {
        return string.Join(", ", _allowedFileExtensionsMain);
    }

    /// <summary>
    /// Returns the complete list of all supported file extensions.
    /// </summary>
    public List<string> GetAllowedExtensionsList() {
        return _allowedFileExtensions;
    }

    /// <summary>
    /// Returns the list of main 3D model extensions.
    /// </summary>
    public List<string> GetMainAllowedExtensionsList() {
        return _allowedFileExtensionsMain;
    }

    /// <summary>
    /// Helper method to strip leading dots and trim whitespace for robust extension comparison.
    /// </summary>
    private string NormalizeExtension(string ext) {
        return ext.Trim().TrimStart('.').ToLowerInvariant();
    }

    #endregion

    // Download from server helper
    /// <summary>
    /// Writes a file from raw byte data into the asset's persistent directory.
    /// </summary>
    private void WriteFile(string assetHash, string fileName, byte[] data) {
        string root = GetPersistentAssetPath(assetHash);
        string path = Path.Combine(root, fileName);
        System.IO.File.WriteAllBytes(path, data);
    }

    /// <summary>
    /// Returns all files for a given asset identified by its fileHash.
    /// Includes OBJ, MTL, textures, or any other files stored in the asset folder.
    /// </summary>
    public List<string> GetAllFilesForAsset(string fileHash) {
        string folderPath = GetPathToFiles(fileHash);

        if (!Directory.Exists(folderPath))
            return new List<string>(); // empty list if folder doesn't exist

        return new List<string>(Directory.GetFiles(folderPath));
    }

    /// <summary>
    /// Returns the persistent data folder path for a given asset hash.
    /// </summary>
    private string GetPersistentAssetPath(string fileHash) {
        string path = Path.Combine(Application.persistentDataPath, fileHash);
        Directory.CreateDirectory(path);
        return path;
    }

    public string GetPathToFiles(string assetHash) {
        return GetPersistentAssetPath(assetHash);
    }

}

/* THE HOLY WORKING CODE */
/*

    /// <summary>
    /// Loads a model from a folder containing OBJ + MTL + texture files.
    /// Automatically parses the MTL and applies textures to the materials.
    /// If there is no MTL or textures, the model will be created without them.
    /// </summary>
    /// <param name="folderPath">Folder containing OBJ, MTL, and texture files.</param>
    /// <param name="objFileName">Name of the OBJ file inside the folder.</param>
    /// <returns>Loaded GameObject with materials applied, deactivated by default.</returns>
    public GameObject LoadObj(string folderOrObjPath) {
        // Determine if the input contains a .obj file
        string folderPath;
        string objFileName;

        if (Path.GetExtension(folderOrObjPath).ToLower() == ".obj") {
            // Full OBJ path provided extract folder and filename
            folderPath = Path.GetDirectoryName(folderOrObjPath);
            objFileName = Path.GetFileName(folderOrObjPath);
        } else {
            // Folder path provided assume the folder contains exactly one OBJ
            folderPath = folderOrObjPath;

            string[] objFiles = Directory.GetFiles(folderPath, "*.obj");
            if (objFiles.Length == 0) {
                Debug.LogError($"No OBJ file found in folder: {folderPath}");
                return null;
            }

            objFileName = Path.GetFileName(objFiles[0]); // take the first OBJ
        }

        string objPath = Path.Combine(folderPath, objFileName);
        string mtlPath = Path.ChangeExtension(objPath, ".mtl");

        if (!File.Exists(objPath)) {
            Debug.LogError($"OBJ file not found: {objPath}");
            return null;
        }

        GameObject loadedObj = new OBJLoader().Load(objPath);
        loadedObj.SetActive(false);

        if (!File.Exists(mtlPath)) {
            Debug.LogWarning($"No MTL file found. OBJ loaded without textures.");
            return loadedObj;
        }

        ApplyMaterialsFromMtl(loadedObj, mtlPath, folderPath);

        return loadedObj;
    }


    /// <summary>
    /// Parses an MTL file and applies the textures to the materials on the loaded model.
    /// </summary>
    private void ApplyMaterialsFromMtl(GameObject obj, string mtlPath, string folder) {
        string[] lines = File.ReadAllLines(mtlPath);
        Dictionary<string, string> materialToTexture = new Dictionary<string, string>();
        string currentMaterial = null;

        foreach (string rawLine in lines) {
            string line = rawLine.Trim();
            if (line.StartsWith("newmtl ")) {
                currentMaterial = line.Substring(7).Trim();
            } else if (line.StartsWith("map_Kd ") && currentMaterial != null) {
                string texName = line.Substring(7).Trim();
                string texPath = Path.Combine(folder, Path.GetFileName(texName));
                materialToTexture[currentMaterial] = texPath;
            }
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            foreach (Material mat in renderer.materials) {
                string cleanName = mat.name.Replace(" (Instance)", "");
                if (!materialToTexture.TryGetValue(cleanName, out string texPath))
                    continue;

                if (!File.Exists(texPath)) {
                    Debug.LogWarning($"Texture not found for {cleanName}: {texPath}");
                    continue;
                }

                byte[] data = File.ReadAllBytes(texPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(data);
                mat.mainTexture = tex;

                Debug.Log($"Applied texture {texPath} -> material {cleanName}");
            }
        }
    }
*/

/* OLD BYTE CREATION */
/*
    public GameObject LoadModel(byte[] file) {
        using (MemoryStream memoryStream = new MemoryStream(file)) {
            // Load the OBJ model from the MemoryStream
            GameObject loadedObject = new OBJLoader().Load(memoryStream);
            loadedObject.SetActive(false);
            return loadedObject;
        }
    }
*/
