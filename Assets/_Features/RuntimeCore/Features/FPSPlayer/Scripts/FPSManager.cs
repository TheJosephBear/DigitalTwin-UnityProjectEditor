using System;
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
    private PlayerSetup playerSetup;
    private bool isInFPSMode = false;

    // Player settings
    // defaultSettings = what the player prefab had before we touched it (filled in Start)
    // currentSettings = what is currently applied / shown in the UI / saved
    // pendingSettings = settings that were loaded before the player existed
    private PlayerSettings defaultSettings = new PlayerSettings();
    private PlayerSettings currentSettings = new PlayerSettings();
    private PlayerSettings pendingSettings;

    public Vector3 SpawnPosition => spawnPosition;
    public PlayerSettings DefaultSettings => defaultSettings.Clone();
    public PlayerSettings CurrentSettings => currentSettings.Clone();

    private void Start() {
        // Instantiate the player up front and keep it inactive
        if (playerPrefab != null) {
            if (SceneLoadingManager.Instance != null) {
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
            playerSetup = playerInstance.GetComponent<PlayerSetup>();

            InitializePlayerSettings();

            playerInstance.SetActive(false);
        }
    }

    private void InitializePlayerSettings() {
        if (playerSetup == null) {
            Debug.LogWarning("FPSManager: player prefab has no PlayerSetup component, player settings will not be applied.");
            return;
        }

        // Read the prefab's own values BEFORE anything gets applied
        defaultSettings = playerSetup.Defaults;

        // Use settings loaded before the player existed, otherwise the defaults
        PlayerSettings initial = pendingSettings ?? defaultSettings;
        pendingSettings = null;

        ApplySettings(initial);
    }

    private void Update() {
        // ESC key completely exits, resets state, and hides the UI
        if (Input.GetKeyDown(KeyCode.Escape)) {
            RequestCloseUI();
            return;
        }

        // Check if player falls below the Y threshold while in FPS mode
        if (isInFPSMode && playerInstance != null && playerInstance.activeSelf) {
            if (playerInstance.transform.position.y < fallThresholdY) {
                RespawnPlayerAtSpawnPoint();
            }
        }
    }

    #region Player Settings

    /// <summary>
    /// Applies the settings to the player, stores them as the current settings
    /// and refreshes the UI with the (sanitized) values.
    /// </summary>
    public void ApplySettings(PlayerSettings settings) {
        if (settings == null) return;

        PlayerSettings requested = settings.Clone();
        requested.Sanitize();

        if (playerSetup != null) {
            playerSetup.Apply(requested);
            currentSettings = playerSetup.Current;   // sanitized against the real capsule radius etc.
        } else {
            // Player doesn't exist yet (e.g. loading happens before Start): remember for later
            currentSettings = requested;
            pendingSettings = requested.Clone();
        }

        if (uiInstance != null) {
            uiInstance.DisplaySettings(currentSettings);
        }
    }

    public void ResetSettingsToDefault() {
        ApplySettings(defaultSettings);
    }

    /// <summary>Clamps settings the same way ApplySettings would, without applying anything.</summary>
    public PlayerSettings SanitizeSettings(PlayerSettings settings) {
        if (playerSetup != null) return playerSetup.Sanitize(settings);

        PlayerSettings s = settings.Clone();
        s.Sanitize();
        return s;
    }

    private bool confirmationOpen = false;

    /// <summary>
    /// Closes the FPS UI. Asks for confirmation first if the settings have unsaved changes.
    /// </summary>
    public void RequestCloseUI(Action<bool> result = null) {
        if (confirmationOpen) return;

        if (uiInstance != null && uiInstance.HasUnsavedChanges) {
            confirmationOpen = true;
            PopUp.Instance.AreYouSurePopUp((sure) => {
                confirmationOpen = false;
                if (sure) {
                    uiInstance.DiscardChanges();
                    ResetAndCloseAll();
                }
                result?.Invoke(sure);
            }, "You have unsaved changes. Discard them and close?");
        } else {
            ResetAndCloseAll();
            result?.Invoke(true);
        }
    }

    /// <summary>
    /// Enters FPS mode. Asks for confirmation first if the settings have unsaved changes
    /// (the player always uses the saved settings, unsaved ones are discarded).
    /// </summary>
    public void RequestEnterFPSMode(Action<bool> result = null) {
        if (confirmationOpen) return;

        if (uiInstance != null && uiInstance.HasUnsavedChanges) {
            confirmationOpen = true;
            PopUp.Instance.AreYouSurePopUp((sure) => {
                confirmationOpen = false;
                if (sure) {
                    uiInstance.DiscardChanges();
                    EnterFPSMode();
                }
                result?.Invoke(sure);
            }, "You have unsaved changes. Enter FPS mode with the saved settings and spawn point instead?");
        } else {
            EnterFPSMode();
            result?.Invoke(true);
        }
    }

    #endregion

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
                uiInstance.Initialize(this);   // also shows the current settings
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
            uiInstance.DisplaySpawnPosition(spawnPosition);
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

        // Re-apply so the player always starts standing with the current settings
        if (playerSetup != null) playerSetup.Apply(currentSettings);

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

    #region Serialization

    public SerializableFPSManager Serialize() {
        SerializableFPSManager serialized = new SerializableFPSManager();
        serialized.SpawnPosition = this.spawnPosition;

        // Only store the settings if they differ from the prefab defaults,
        // so untouched players keep following the prefab.
        serialized.Settings = currentSettings.ApproximatelyEquals(defaultSettings)
            ? null
            : currentSettings.Clone();

        return serialized;
    }

    public void Deserialize(SerializableFPSManager serializedManager) {
        if (serializedManager == null) return;

        SetSpawnPosition(serializedManager.SpawnPosition);

        // No stored settings (untouched or old save) -> prefab defaults
        PlayerSettings loaded = serializedManager.Settings;

        if (loaded == null && playerSetup == null) {
            // Player not created yet: Start() will fall back to the defaults on its own
            pendingSettings = null;
            return;
        }

        ApplySettings(loaded ?? defaultSettings);
    }

    #endregion
}

[Serializable]
public class SerializableFPSManager {
    [SerializeField] private float posX, posY, posZ;

    // Player settings (hasSettings == false -> use the prefab defaults; also the case for old saves)
    [SerializeField] private bool hasSettings;
    [SerializeField] private float playerSpeed;
    [SerializeField] private bool playerJumpAllowed;
    [SerializeField] private float playerJumpStrength;
    [SerializeField] private bool playerCrouchAllowed;
    [SerializeField] private float playerHeight;

    public Vector3 SpawnPosition {
        get => new Vector3(posX, posY, posZ);
        set {
            posX = value.x;
            posY = value.y;
            posZ = value.z;
        }
    }

    public PlayerSettings Settings {
        get {
            if (!hasSettings) return null;
            return new PlayerSettings {
                speed = playerSpeed,
                jumpAllowed = playerJumpAllowed,
                jumpStrength = playerJumpStrength,
                crouchAllowed = playerCrouchAllowed,
                playerHeight = playerHeight
            };
        }
        set {
            hasSettings = value != null;
            if (value == null) return;

            playerSpeed = value.speed;
            playerJumpAllowed = value.jumpAllowed;
            playerJumpStrength = value.jumpStrength;
            playerCrouchAllowed = value.crouchAllowed;
            playerHeight = value.playerHeight;
        }
    }
}
