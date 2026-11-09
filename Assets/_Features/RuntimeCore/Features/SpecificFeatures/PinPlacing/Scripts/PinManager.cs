using System.Collections.Generic;
using UnityEngine;

public class PinManager: Singleton<PinManager> {

    [Header("Prefabs & Setup")]
    public GameObject PinPrefab;
    public GameObject PinUIPrefab;
    public GameObject PinContextUIPrefab;
    [Tooltip("Read only context UI the viewer opens instead of the editing one")]
    public GameObject PinInfoUIPrefab;

    [Header("Viewer")]
    [Tooltip("Creates the pin list UI in the viewer too (the editor always has it)")]
    public bool InstantiatePinUIInViewer = false;

    [Header("Pin camera")]
    [Tooltip("Distance of a pin's vcam from the pin (shorter when a wall is in the way)")]
    public float VcamDistance = 10f;
    [Tooltip("How many degrees the pin's vcam looks down at the pin (other angles are tried when geometry is in the way)")]
    [Range(5f, 85f)]
    public float VcamPitch = 45f;
    [Tooltip("[min, max] camera-to-pin distance in which clicking a pin only turns the camera to it instead of flying to its vcam")]
    public Vector2 TurnOnlyDistanceRange = new Vector2(40f, 100000f);
    [Tooltip("Geometry the pin vcams can't be placed inside of")]
    public LayerMask VcamObstacleLayers = ~0;
    [Tooltip("Clicking a pin flies the camera to its vcam in the editor, this also enables it in the viewer")]
    public bool EnableVcamInViewer = false;

    private List<PinObject> _pins = new List<PinObject>();
    private PinObject _activePin;
    private PinUI _pinUIInstance;
    private PinContextUI _contextUIInstance;
    private PinInfoUI _infoUIInstance;
    private PinPlacingManager _placingManager;
    private SceneType _sceneToInstantiate = SceneType.Editing;

    protected override void Awake() {
        base.Awake();
        _placingManager = GetComponent<PinPlacingManager>();
        if (SceneLoadingManager.Instance != null) _sceneToInstantiate = SceneLoadingManager.Instance.GetActiveScene();
    }

    private void OnEnable() {
        if (_pinUIInstance == null && SceneLoadingManager.Instance != null) {
            InitializeUI();
        }
    }

    /// <summary>
    /// True in the Viewing scene. Decided by the scene this manager is in, because OnEnable creates the UI
    /// before the viewer is the active scene and possibly before MainManagerBase.Instance exists.
    /// </summary>
    public bool IsViewer => SceneList.Instance != null && gameObject.scene.name == SceneList.Instance.GetScene(SceneType.Viewing).SceneName;

    /// <summary>False in the viewer unless InstantiatePinUIInViewer is on.</summary>
    public bool HasPinUI => _pinUIInstance != null;

    public void InitializeUI() {
        if (IsViewer) {
            _sceneToInstantiate = SceneType.Viewing;
            if (!InstantiatePinUIInViewer) return;
        } else {
            _sceneToInstantiate = SceneLoadingManager.Instance.GetActiveScene(); // Just to be sure...
        }

        _pinUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(PinUIPrefab, _sceneToInstantiate).GetComponent<PinUI>();
        _pinUIInstance.Initialize(this);
    }

    #region Pin management

    /// <summary>
    /// Spawns a pin without adding it to the project, used as the preview while placing a new pin.
    /// </summary>
    public PinObject CreatePinObject(Vector3 position) {
        if (SceneLoadingManager.Instance == null || PinPrefab == null) return null;

        GameObject spawnedObj = SceneLoadingManager.Instance.InstantiateObjectInScene(PinPrefab, position, _sceneToInstantiate);
        PinObject newPin = spawnedObj.GetComponent<PinObject>();
        newPin.SetName("Pin " + (_pins.Count + 1));
        return newPin;
    }

    public void RegisterPin(PinObject pin, bool deserialization = false) {
        _pins.Add(pin);
        RefreshPinButtons();

        if (!deserialization) MarkUnsavedChanges();
    }

    public void DeletePin(PinObject pin) {
        PopUp.Instance.AreYouSurePopUp((sure) => {
            if (!sure) return;

            if (pin == _activePin) CloseContextUI();
            _pins.Remove(pin);
            Destroy(pin.gameObject);
            RefreshPinButtons();
            MarkUnsavedChanges();
        }, $"Chcete smazat pin {pin.Name}?");
    }

    public void SwapPins(int index, int newIndex) {
        PinObject temp = _pins[index];
        _pins[index] = _pins[newIndex];
        _pins[newIndex] = temp;
        MarkUnsavedChanges();
    }

    public List<PinObject> GetPins() => _pins;

    public PinObject GetActivePin() => _activePin;

    public bool IsPlacing => _placingManager != null && _placingManager.IsPlacing;

    public void MarkUnsavedChanges() {
        if (MainManagerBase.Instance is EditorManager editorManager) editorManager.ToggleUnsavedChanges(true);
    }

    #endregion

    #region Pin editing

    /// <summary>
    /// Opens the context UI for the pin, always loading that pin's data.
    /// </summary>
    public void SelectPin(PinObject pin) {
        if (pin == null || pin == _activePin) return;

        // A selected view point is left first (like its cancel button, asking about unsaved changes), so only the pin stays selected
        if (!IsViewer && MainManagerBase.Instance != null && MainManagerBase.Instance.ActiveState == AppState.ViewActive && ViewManager.Instance != null) {
            ViewManager.Instance.ExitViewMoving(save: false, onExited: () => SelectPin(pin));
            return;
        }

        if (_contextUIInstance != null && _contextUIInstance.HasUnsavedChanges) {
            PopUp.Instance.AreYouSurePopUp((sure) => {
                if (!sure) {
                    ToggleContextUI(true); // Could have been hidden by placing a new pin
                    return;
                }

                _contextUIInstance.Revert();
                OpenContextUI(pin);
            }, "Neuložené změny, přejete si odejít?");
            return;
        }

        OpenContextUI(pin);
    }

    private void OpenContextUI(PinObject pin) {
        if (_activePin != null) _activePin.SetOutline(false);
        _activePin = pin;

        if (IsViewer) {
            // The viewer only shows the pin's content
            if (_infoUIInstance == null)
                _infoUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(PinInfoUIPrefab, _sceneToInstantiate).GetComponent<PinInfoUI>();
            _infoUIInstance.Initialize(pin, this);
        } else {
            if (_contextUIInstance == null)
                _contextUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(PinContextUIPrefab, _sceneToInstantiate).GetComponent<PinContextUI>();
            _contextUIInstance.Initialize(pin, this);
        }
        ToggleContextUI(true);

        pin.SetOutline(true);
        RefreshPinButtons();
    }

    /// <summary>
    /// Flies the camera to the pin's vcam (Cinemachine blend), then hands the camera back to the free cam there.
    /// Within TurnOnlyDistanceRange the vcam is where the camera is, so the blend only turns the camera to the pin.
    /// </summary>
    public void FocusPin(PinObject pin) {
        if (pin == null || pin.Vcam == null || CameraManager.Instance == null || MainManagerBase.Instance == null) return;
        if (MainManagerBase.Instance is ViewingManager && !EnableVcamInViewer) return;
        if (MainManagerBase.Instance.ActiveState != AppState.Freecam) return; // other states drive the camera themselves

        float distance = Camera.main != null ? Vector3.Distance(Camera.main.transform.position, pin.transform.position) : 0f;
        if (distance >= TurnOnlyDistanceRange.x && distance <= TurnOnlyDistanceRange.y) {
            pin.AimVcamFromCamera();
        } else {
            pin.PlaceVcam(VcamDistance, VcamPitch, VcamObstacleLayers);
        }
        CameraManager.Instance.FlyToVcam(pin.Vcam);
    }

    public void CloseContextUI() {
        if (_activePin != null) _activePin.SetOutline(false);
        _activePin = null;

        ToggleContextUI(false);
        if (_pinUIInstance != null) _pinUIInstance.OnExitEditing();
    }

    public void StartPlacingNewPin() {
        _placingManager.StartPlacing(null);
    }

    public void StartMovingPin(PinObject pin) {
        pin.SetOutline(false);
        _placingManager.StartPlacing(pin);
    }

    public void OnPlacingFinished(PinObject pin, bool isNew, bool confirmed) {
        if (isNew) {
            if (!confirmed) {
                Destroy(pin.gameObject);
                if (_activePin != null) ToggleContextUI(true); // Placing hid the context UI of the pin being edited
                return;
            }

            RegisterPin(pin);
            SelectPin(pin);
            return;
        }

        // Moving continues the context UI's edit session, so a cancel there still reverts the move
        if (pin == _activePin) {
            pin.SetOutline(true);
            ToggleContextUI(true);
            _contextUIInstance.OnReturnFromMoving();
        }
    }

    #endregion

    #region UI management

    public void TogglePinUI(bool show) {
        if (_pinUIInstance != null) _pinUIInstance.gameObject.SetActive(show);
    }

    // Only one of them exists: the editing one in the editor, the read only one in the viewer
    public void ToggleContextUI(bool show) {
        if (_contextUIInstance != null) _contextUIInstance.gameObject.SetActive(show);
        if (_infoUIInstance != null) _infoUIInstance.gameObject.SetActive(show);
    }

    public void RefreshPinButtons() {
        if (_pinUIInstance != null) _pinUIInstance.UpdatePinButtonList();
    }

    #endregion

    public void ClearEverything() {
        _placingManager.CancelPlacing();
        CloseContextUI();
        Utilities.DestroyAllGameObjects(_pins);
        if (_pinUIInstance != null) _pinUIInstance.ClearPinButtonList();
    }

    #region Serialization

    public SerializablePinManager Serialize() {
        List<SerializablePin> list = new List<SerializablePin>();
        foreach (PinObject pin in _pins) {
            list.Add(pin.Serialize());
        }
        return new SerializablePinManager { Pins = list };
    }

    public void Deserialize(SerializablePinManager data) {
        if (data == null || data.Pins == null) {
            Debug.Log("Pin manager deserialization skipped: Source data is null");
            return;
        }

        foreach (SerializablePin pinData in data.Pins) {
            PinObject newPin = CreatePinObject(pinData.position);
            if (newPin == null) continue;

            newPin.Deserialize(pinData);
            RegisterPin(newPin, deserialization: true);
        }
    }

    #endregion
}
