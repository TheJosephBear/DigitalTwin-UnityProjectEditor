using UnityEngine;
using UnityEngine.Profiling;
using TMPro;
using System.Collections.Generic;
using UnityEditor;

public class PerformanceTracker: MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private GameObject _dataContainer;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text _fpsText;
    [SerializeField] private TMP_Text _memoryText;
    [SerializeField] private TMP_Text _batchingText;
    [SerializeField] private TMP_Text _drawCallsText;
    [SerializeField] private TMP_Text _materialInstancingText;

    [Header("Settings")]
    [SerializeField] private float _updateInterval = 0.25f; // Update UI 4 times per second to prevent text rebuilding lag

    private Camera _mainCam;
    private float _deltaTime;
    private float _timer;

    // Cached collection to eliminate GC Allocations every frame
    private HashSet<Material> _uniqueSharedMaterials = new HashSet<Material>();

    void Start() {
        _mainCam = Camera.main;
        if (_dataContainer != null) {
            _dataContainer.SetActive(false); // Hide data view by default
        }
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
        if (_mainCam == null) _mainCam = Camera.main;

        // --- 1. FPS & Frame Time ---
        float fps = 1.0f / _deltaTime;
        float frameTimeMs = _deltaTime * 1000.0f;
        if (_fpsText != null)
            _fpsText.text = $"FPS: {fps:F0} ({frameTimeMs:F1} ms)";

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

        if (_memoryText != null)
            _memoryText.text = $"RAM: {totalRamMb} MB | Mesh VRAM: {(visibleMeshVramBytes / 1024f / 1024f):F2} MB";

        // --- 3 & 4. Draw Calls & Batching ---
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int activeBatches = UnityStats.batches;
        int setPassCalls = UnityStats.setPassCalls;

        if (_drawCallsText != null)
            _drawCallsText.text = $"Batches: {activeBatches} | SetPass: {setPassCalls}";

        if (_batchingText != null) {
            float batchRatio = visibleRenderers > 0 ? (float)activeBatches / visibleRenderers : 0;
            _batchingText.text = $"Batching Efficiency: {batchRatio:F2} batches/renderer ({visibleRenderers} Visible Renderers)";
        }
#else
        if (_drawCallsText != null)
            _drawCallsText.text = "Batches: [Dev Build Req.]";
        if (_batchingText != null)
            _batchingText.text = $"Visible Renderers: {visibleRenderers} ({visibleSubmeshes} Submeshes)";
#endif

        // --- 5. Material Instancing ---
        if (_materialInstancingText != null) {
            if (instancedMaterialCount > 0) {
                _materialInstancingText.text = $"Materials: {_uniqueSharedMaterials.Count} | <color=red>Leaked Instances: {instancedMaterialCount}</color>";
            } else {
                _materialInstancingText.text = $"Materials: {_uniqueSharedMaterials.Count} | Instanced Leaks: 0";
            }
        }
    }
}
