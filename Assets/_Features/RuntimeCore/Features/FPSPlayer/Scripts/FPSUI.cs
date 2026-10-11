using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FPSUI: MonoBehaviour {
    [Header("UI View Containers")]
    [SerializeField] private GameObject _settingsView;
    [SerializeField] private GameObject _placingModeView;
    [SerializeField] private GameObject _fpsModeView;

    [Header("UI Input references")]
    [SerializeField] private TMP_InputField _heightInput;
    [SerializeField] private TMP_InputField _speedInput;
    [SerializeField] private TMP_InputField _jumpStrInput;
    [SerializeField] private UISwitcher.UISwitcher _jumpAllowedInput;
    [SerializeField] private UISwitcher.UISwitcher _crouchAllowedInput; // optional

    [Header("Buttons")]
    [Tooltip("Optional: only interactable while there are unsaved changes. " +
             "Its onClick should call OnSaveButtonClicked.")]
    [SerializeField] private Button _saveButton;

    [Header("Placing Mode Settings")]
    [SerializeField] private LayerMask _placementRaycastMask = ~0; // Default: Everything
    [SerializeField] private GameObject _spawnPreviewPrefab; // 3D ghost object/marker

    private FPSManager _manager;
    private bool _isPlacingMode = false;
    private bool _inputsSubscribed = false;
    private GameObject _spawnPreviewInstance;

    // What the fields currently show. Only becomes the player's settings on Save.
    private PlayerSettings _draft = new PlayerSettings();
    // Spawn point shown by the preview marker. Only becomes the real spawn point on Save.
    private Vector3 _draftSpawn;

    /// <summary>True if the settings or the spawn point differ from what is saved.</summary>
    public bool HasUnsavedChanges =>
        _manager != null
        && (!_draft.ApproximatelyEquals(_manager.CurrentSettings) || _draftSpawn != _manager.SpawnPosition);

    public void Initialize(FPSManager managerReference) {
        _manager = managerReference;
        SubscribeInputs();
        DiscardChanges();
        ShowSettingsView();
    }

    private void OnEnable() {
        // Opening the UI always starts from the saved values
        if (_manager != null) DiscardChanges();
    }

    private void Update() {
        if (_isPlacingMode) {
            HandlePlacingInput();
        }
    }

    #region Player Settings

    /// <summary>
    /// Loads the given (saved) settings into the fields, discarding any draft.
    /// No change events are triggered.
    /// </summary>
    public void DisplaySettings(PlayerSettings settings) {
        if (settings == null) return;

        _draft = settings.Clone();
        RefreshFields();
        RefreshSaveButton();
    }

    /// <summary>
    /// Called by the manager when the saved spawn point changes (save, load).
    /// Resets the draft spawn point and moves the marker there.
    /// </summary>
    public void DisplaySpawnPosition(Vector3 position) {
        _draftSpawn = position;
        ShowSpawnVisualAt(_draftSpawn);
        RefreshSaveButton();
    }

    /// <summary>Throws away all unsaved changes (settings and spawn point) and shows the saved state again.</summary>
    public void DiscardChanges() {
        if (_manager == null) return;
        DisplaySettings(_manager.CurrentSettings);
        DisplaySpawnPosition(_manager.SpawnPosition);
    }

    /// <summary>Hook to the Save button.</summary>
    public void OnSaveButtonClicked() {
        if (_manager == null) return;
        _manager.SetSpawnPosition(_draftSpawn);   // manager calls DisplaySpawnPosition with the saved value
        _manager.ApplySettings(_draft);           // manager applies, then calls DisplaySettings with the saved values
    }

    /// <summary>Hook to an optional "Reset to defaults" button. Loads the defaults as an unsaved draft.</summary>
    public void OnResetSettingsClicked() {
        if (_manager == null) return;
        _draft = _manager.SanitizeSettings(_manager.DefaultSettings);
        RefreshFields();
        RefreshSaveButton();
    }

    private void RefreshFields() {
        SetText(_heightInput, _draft.playerHeight);
        SetText(_speedInput, _draft.speed);
        SetText(_jumpStrInput, _draft.jumpStrength);

        if (_jumpAllowedInput != null) _jumpAllowedInput.SetWithoutNotify(_draft.jumpAllowed);
        if (_crouchAllowedInput != null) _crouchAllowedInput.SetWithoutNotify(_draft.crouchAllowed);
    }

    private void RefreshSaveButton() {
        if (_saveButton != null) _saveButton.interactable = HasUnsavedChanges;
    }

    private void SubscribeInputs() {
        if (_inputsSubscribed) return;
        _inputsSubscribed = true;

        if (_heightInput != null) _heightInput.onEndEdit.AddListener(OnTextInputEdited);
        if (_speedInput != null) _speedInput.onEndEdit.AddListener(OnTextInputEdited);
        if (_jumpStrInput != null) _jumpStrInput.onEndEdit.AddListener(OnTextInputEdited);
        if (_jumpAllowedInput != null) _jumpAllowedInput.onValueChanged.AddListener(OnToggleInputChanged);
        if (_crouchAllowedInput != null) _crouchAllowedInput.onValueChanged.AddListener(OnToggleInputChanged);
    }

    private void UnsubscribeInputs() {
        if (!_inputsSubscribed) return;
        _inputsSubscribed = false;

        if (_heightInput != null) _heightInput.onEndEdit.RemoveListener(OnTextInputEdited);
        if (_speedInput != null) _speedInput.onEndEdit.RemoveListener(OnTextInputEdited);
        if (_jumpStrInput != null) _jumpStrInput.onEndEdit.RemoveListener(OnTextInputEdited);
        if (_jumpAllowedInput != null) _jumpAllowedInput.onValueChanged.RemoveListener(OnToggleInputChanged);
        if (_crouchAllowedInput != null) _crouchAllowedInput.onValueChanged.RemoveListener(OnToggleInputChanged);
    }

    private void OnTextInputEdited(string _) => UpdateDraftFromFields();
    private void OnToggleInputChanged(bool _) => UpdateDraftFromFields();

    // Edits only change the draft. Nothing reaches the player until Save.
    private void UpdateDraftFromFields() {
        if (_manager == null) return;

        // Start from the draft so unparsable / untouched fields keep their value
        PlayerSettings draft = _draft.Clone();

        if (TryReadFloat(_heightInput, out float height)) draft.playerHeight = height;
        if (TryReadFloat(_speedInput, out float speed)) draft.speed = speed;
        if (TryReadFloat(_jumpStrInput, out float jump)) draft.jumpStrength = jump;
        if (_jumpAllowedInput != null) draft.jumpAllowed = _jumpAllowedInput.isOn;
        if (_crouchAllowedInput != null) draft.crouchAllowed = _crouchAllowedInput.isOn;

        _draft = _manager.SanitizeSettings(draft);
        RefreshFields();        // normalizes what the user typed
        RefreshSaveButton();
    }

    private static bool TryReadFloat(TMP_InputField field, out float value) {
        value = 0f;
        if (field == null) return false;

        string text = field.text.Trim().Replace(',', '.');
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            && float.IsFinite(value);
    }

    private static void SetText(TMP_InputField field, float value) {
        if (field != null) field.SetTextWithoutNotify(value.ToString("0.##", CultureInfo.InvariantCulture));
    }

    #endregion

    private void HandlePlacingInput() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast to track cursor in real time
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placementRaycastMask)) {
            EnsurePreviewInstance();

            if (_spawnPreviewInstance != null) {
                _spawnPreviewInstance.SetActive(true);
                _spawnPreviewInstance.transform.position = hit.point;
            }

            // Left Click to set position
            if (Input.GetMouseButtonDown(0)) {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
                    return;
                }

                // Only changes the draft, the real spawn point is set on Save
                _draftSpawn = hit.point;
                RefreshSaveButton();

                ExitPlacingMode();
            }
        }
    }

    /// <summary>
    /// Forces the 3D spawn visual to be active at a specific world position.
    /// </summary>
    public void ShowSpawnVisualAt(Vector3 position) {
        EnsurePreviewInstance();

        if (_spawnPreviewInstance != null) {
            _spawnPreviewInstance.SetActive(true);
            _spawnPreviewInstance.transform.position = position;
        }
    }

    /// <summary>
    /// Hides the 3D spawn visual.
    /// </summary>
    public void HideSpawnVisual() {
        if (_spawnPreviewInstance != null) {
            _spawnPreviewInstance.SetActive(false);
        }
    }

    private void EnsurePreviewInstance() {
        if (_spawnPreviewInstance == null && _spawnPreviewPrefab != null) {
            _spawnPreviewInstance = Instantiate(_spawnPreviewPrefab);
        }
    }

    // --- View Navigation Methods ---

    public void EnterPlacingMode() {
        _isPlacingMode = true;
        SetViewsActive(settings: false, placing: true, fps: false);
    }

    public void ExitPlacingMode() {
        _isPlacingMode = false;

        // Show the marker at the (possibly unsaved) spawn point
        if (_manager != null) {
            ShowSpawnVisualAt(_draftSpawn);
        }

        ShowSettingsView();
    }

    public void ShowSettingsView() {
        _isPlacingMode = false;
        SetViewsActive(settings: true, placing: false, fps: false);

        if (_manager != null) {
            ShowSpawnVisualAt(_draftSpawn);
        }
    }

    /// <summary>X button: asks for confirmation if there are unsaved changes.</summary>
    public void OnX() {
        _manager.RequestCloseUI();
    }

    public void ShowFPSModeView() {
        _isPlacingMode = false;
        HideSpawnVisual();
        SetViewsActive(settings: false, placing: false, fps: true);
    }

    public void OnEnterFPSButtonClicked() {
        if (_manager != null) {
            _manager.RequestEnterFPSMode();
        }
    }

    private void SetViewsActive(bool settings, bool placing, bool fps) {
        if (_settingsView != null) _settingsView.SetActive(settings);
        if (_placingModeView != null) _placingModeView.SetActive(placing);
        if (_fpsModeView != null) _fpsModeView.SetActive(fps);
    }

    private void OnDisable() {
        HideSpawnVisual();
    }

    private void OnDestroy() {
        UnsubscribeInputs();

        if (_spawnPreviewInstance != null) {
            Destroy(_spawnPreviewInstance);
        }
    }
}
