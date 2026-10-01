using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class PerformanceTracker: MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private GameObject _dataContainer;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text _text;

    [Header("Settings")]
    [SerializeField] private float _updateInterval = 0.25f; // Update UI 4 times per second to prevent text rebuilding lag

    private Camera _mainCam;
    private float _deltaTime;
    private float _timer;
    private ProfilerRecorder _batchesRecorder;
    private ProfilerRecorder _setPassCallsRecorder;

    // Cached collections to eliminate GC Allocations every frame
    private HashSet<Material> _uniqueSharedMaterials = new HashSet<Material>();
    private StringBuilder _sb = new StringBuilder(256);

    void Start() {
        _mainCam = Camera.main;
        _batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        _setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        if (_dataContainer != null) {
            _dataContainer.SetActive(false); // Hide data view by default
        }
    }

    void OnDisable() {
        _batchesRecorder.Dispose();
        _setPassCallsRecorder.Dispose();
    }

    void Update() {
        // 1. Accumulate FPS smoothing math continuously
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

        // Skip calculation if the view container is toggled off
        if (_dataContainer == null || !_dataContainer.activeSelf) return;

        _timer += Time.unscaledDeltaTime;
        if (_timer >= _updateInterval) {
            _timer = 0f;
            UpdatePerformanceData();
        }
    }

    public void ToggleDataView() {
        if (_dataContainer != null) {
            _dataContainer.SetActive(!_dataContainer.activeSelf);
        }
    }

    public void HideDataView() {
        if (_dataContainer != null) {
            _dataContainer.SetActive(false);
        }
    }

    private void UpdatePerformanceData() {
        if (_text == null) return;
        if (_mainCam == null) _mainCam = Camera.main;
        _sb.Clear();

        // --- 1. FPS & Frame Time ---
        float fps = 1.0f / _deltaTime;
        float frameTimeMs = _deltaTime * 1000.0f;
        _sb.Append($"FPS: {fps:F0} ({frameTimeMs:F1} ms)").Append('\n');

        // --- 2. Memory Impact ---
        long totalRamMb = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
        long visibleMeshVramBytes = 0;

        // Frustum scan to target visible models
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(_mainCam);
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        int visibleRenderers = 0;
        int visibleSubmeshes = 0;
        int instancedMaterialCount = 0;
        _uniqueSharedMaterials.Clear();

        foreach (Renderer rend in allRenderers) {
            if (rend.enabled && GeometryUtility.TestPlanesAABB(frustumPlanes, rend.bounds)) {
                visibleRenderers++;
                Material[] sharedMats = rend.sharedMaterials;
                visibleSubmeshes += sharedMats.Length;

                foreach (Material mat in sharedMats) {
                    if (mat != null) {
                        // Material Instancing Check: Leaked copies append "(Instance)"
                        if (mat.name.EndsWith("(Instance)")) {
                            instancedMaterialCount++;
                        }
                        _uniqueSharedMaterials.Add(mat);
                    }
                }

                MeshFilter mf = rend.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null) {
                    visibleMeshVramBytes += Profiler.GetRuntimeMemorySizeLong(mf.sharedMesh);
                }
            }
        }

        _sb.Append($"RAM: {totalRamMb} MB").Append('\n');
        _sb.Append($"Mesh VRAM: {visibleMeshVramBytes / 1024f / 1024f:F2} MB").Append('\n');

        // --- 3 & 4. Batching & Draw Calls ---
        long activeBatches = _batchesRecorder.Valid ? _batchesRecorder.LastValue : 0;
        long setPassCalls = _setPassCallsRecorder.Valid ? _setPassCallsRecorder.LastValue : 0;
        float batchRatio = visibleRenderers > 0 ? (float)activeBatches / visibleRenderers : 0;

        _sb.Append($"Visible Renderers: {visibleRenderers}").Append('\n');
        _sb.Append($"Batches: {activeBatches}").Append('\n');
        // _sb.Append($"Batching Efficiency: {batchRatio:F2} batches/renderer").Append('\n');
        _sb.Append($"SetPass: {setPassCalls}").Append('\n');

        // --- 5. Material Instancing ---
        _sb.Append($"Materials: {_uniqueSharedMaterials.Count}").Append('\n');
        _sb.Append($"Visible Submeshes: {visibleSubmeshes}").Append('\n');
        if (instancedMaterialCount > 0) {
            _sb.Append($"<color=red>Leaked Instances: {instancedMaterialCount}</color>");
        } else {
            _sb.Append("Instanced Leaks: 0");
        }

        _text.SetText(_sb);
    }
}
