using UnityEngine;

public class FPSManager: Singleton<FPSManager> {
    [Header("Prefabs & Spawn Settings")]
    [SerializeField] private GameObject uiPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    [Header("Fall & Respawn Settings")]
    [Tooltip("If the player's Y position falls below this value, they will be respawned.")]
    [SerializeField] private float fallThresholdY = -10f;
    [Tooltip("Height offset added to the spawn position when respawning/placing.")]
    [SerializeField] private float respawnHeightOffset = 1.0f;

    private FPSUI uiInstance;
    private GameObject playerInstance;
    private Rigidbody playerRigidbody;
    private CharacterController playerController;
    private bool isInFPSMode = false;

    public Vector3 SpawnPosition => spawnPosition;

    private void Start() {
        // Instantiate the player up front and keep it inactive
        if (playerPrefab != null) {
            if(SceneLoadingManager.Instance != null) {
                SceneType sceneToLoadInto;
                if (MainManagerBase.Instance is EditorManager) sceneToLoadInto = SceneType.Editing; else sceneToLoadInto = SceneType.Viewing;
                playerInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(playerPrefab, spawnPosition, Quaternion.identity, sceneToLoadInto);
            } else {
                playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            }
            
            print(playerInstance);

            // Cache references for resetting physics/movement on respawn
            playerRigidbody = playerInstance.GetComponent<Rigidbody>();
            playerController = playerInstance.GetComponent<CharacterController>();

            playerInstance.SetActive(false);
        }
    }

    private void Update() {
        // ESC key completely exits, resets state, and hides the UI
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ResetAndCloseAll();
            return;
        }

        // Check if player falls below the Y threshold while in FPS mode
        if (isInFPSMode && playerInstance != null && playerInstance.activeSelf) {
            if (playerInstance.transform.position.y < fallThresholdY) {
                RespawnPlayerAtSpawnPoint();
            }
        }
    }

    /// <summary>
    /// Resets player position slightly above the spawn point and clears physics momentum.
    /// </summary>
    private void RespawnPlayerAtSpawnPoint() {
        Vector3 targetPosition = spawnPosition + Vector3.up * respawnHeightOffset;

        // Disable CharacterController temporarily to allow position override
        if (playerController != null) {
            playerController.enabled = false;
            playerInstance.transform.position = targetPosition;
            playerController.enabled = true;
        } else {
            playerInstance.transform.position = targetPosition;
        }

        // Reset Rigidbody velocity if using physics movement
        if (playerRigidbody != null) {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Shows or hides the FPS UI canvas. Instantiates on first use.
    /// </summary>
    public void ToggleUI(bool toggleOn) {
        if (uiInstance == null && uiPrefab != null) {
            GameObject instantiatedUI = Instantiate(uiPrefab);
            uiInstance = instantiatedUI.GetComponent<FPSUI>();

            if (uiInstance != null) {
                uiInstance.Initialize(this);
            }
        }

        if (uiInstance != null) {
            uiInstance.gameObject.SetActive(toggleOn);

            if (toggleOn) {
                uiInstance.ShowSpawnVisualAt(spawnPosition);
            } else {
                ResetAndCloseAll();
            }
        }
    }

    /// <summary>
    /// Updates the player's spawn location.
    /// </summary>
    public void SetSpawnPosition(Vector3 position) {
        spawnPosition = position;

        if (playerInstance != null && !isInFPSMode) {
            playerInstance.transform.position = spawnPosition;
        }

        if (uiInstance != null) {
            uiInstance.ShowSpawnVisualAt(spawnPosition);
        }
    }

    /// <summary>
    /// Spawns/Activates the FPS Player and updates UI state.
    /// </summary>
    public void EnterFPSMode() {
        if (playerInstance == null) return;

        isInFPSMode = true;

        // Hide the spawn visual when entering FPS mode
        if (uiInstance != null) {
            uiInstance.HideSpawnVisual();
            uiInstance.ShowFPSModeView();
        }

        RespawnPlayerAtSpawnPoint();
        playerInstance.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        MainManagerBase.Instance?.ChangeState(AppState.FPS);
    }

    /// <summary>
    /// Deactivates player, exits placing mode, hides UI, and resets mouse cursor.
    /// </summary>
    public void ResetAndCloseAll() {
        isInFPSMode = false;

        if (playerInstance != null) {
            playerInstance.SetActive(false);
        }

        if (uiInstance != null) {
            uiInstance.ExitPlacingMode();
            uiInstance.HideSpawnVisual();
            uiInstance.gameObject.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MainManagerBase.Instance?.ChangeState(AppState.Freecam);
    }
}
