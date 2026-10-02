using UnityEngine;
using UnityEngine.EventSystems;

public class FPSUI: MonoBehaviour {
    [Header("UI View Containers")]
    [SerializeField] private GameObject settingsView;
    [SerializeField] private GameObject placingModeView;
    [SerializeField] private GameObject fpsModeView;

    [Header("Placing Mode Settings")]
    [SerializeField] private LayerMask placementRaycastMask = ~0; // Default: Everything
    [SerializeField] private GameObject spawnPreviewPrefab; // 3D ghost object/marker

    private FPSManager manager;
    private bool isPlacingMode = false;
    private GameObject spawnPreviewInstance;

    public void Initialize(FPSManager managerReference) {
        manager = managerReference;
        ShowSettingsView();
    }

    private void OnEnable() {
        if (manager != null) {
            ShowSpawnVisualAt(manager.SpawnPosition);
        }
    }

    private void Update() {
        if (isPlacingMode) {
            HandlePlacingInput();
        }
    }

    private void HandlePlacingInput() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast to track cursor in real time
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementRaycastMask)) {
            EnsurePreviewInstance();

            if (spawnPreviewInstance != null) {
                spawnPreviewInstance.SetActive(true);
                spawnPreviewInstance.transform.position = hit.point;
            }

            // Left Click to set position
            if (Input.GetMouseButtonDown(0)) {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
                    return;
                }

                if (manager != null) {
                    manager.SetSpawnPosition(hit.point);
                }

                ExitPlacingMode();
            }
        }
    }

    /// <summary>
    /// Forces the 3D spawn visual to be active at a specific world position.
    /// </summary>
    public void ShowSpawnVisualAt(Vector3 position) {
        EnsurePreviewInstance();

        if (spawnPreviewInstance != null) {
            spawnPreviewInstance.SetActive(true);
            spawnPreviewInstance.transform.position = position;
        }
    }

    /// <summary>
    /// Hides the 3D spawn visual.
    /// </summary>
    public void HideSpawnVisual() {
        if (spawnPreviewInstance != null) {
            spawnPreviewInstance.SetActive(false);
        }
    }

    private void EnsurePreviewInstance() {
        if (spawnPreviewInstance == null && spawnPreviewPrefab != null) {
            spawnPreviewInstance = Instantiate(spawnPreviewPrefab);
        }
    }

    // --- View Navigation Methods ---

    public void EnterPlacingMode() {
        isPlacingMode = true;
        SetViewsActive(settings: false, placing: true, fps: false);
    }

    public void ExitPlacingMode() {
        isPlacingMode = false;

        // Return the visual to the saved spawn position
        if (manager != null) {
            ShowSpawnVisualAt(manager.SpawnPosition);
        }

        ShowSettingsView();
    }

    public void ShowSettingsView() {
        isPlacingMode = false;
        SetViewsActive(settings: true, placing: false, fps: false);

        if (manager != null) {
            ShowSpawnVisualAt(manager.SpawnPosition);
        }
    }

    public void OnX() {
        manager.ToggleUI(false);
    }

    public void ShowFPSModeView() {
        isPlacingMode = false;
        HideSpawnVisual();
        SetViewsActive(settings: false, placing: false, fps: true);
    }

    public void OnEnterFPSButtonClicked() {
        if (manager != null) {
            manager.EnterFPSMode();
        }
    }

    private void SetViewsActive(bool settings, bool placing, bool fps) {
        if (settingsView != null) settingsView.SetActive(settings);
        if (placingModeView != null) placingModeView.SetActive(placing);
        if (fpsModeView != null) fpsModeView.SetActive(fps);
    }

    private void OnDisable() {
        HideSpawnVisual();
    }

    private void OnDestroy() {
        if (spawnPreviewInstance != null) {
            Destroy(spawnPreviewInstance);
        }
    }
}
