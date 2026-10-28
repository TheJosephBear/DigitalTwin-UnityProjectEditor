using System.Collections.Generic;
using UnityEngine;

public class PinManager: Singleton<PinManager> {

    [Header("Prefabs & Setup")]
    public GameObject PinPrefab;
    public GameObject PinUIPrefab;
    public GameObject PinContextUIPrefab;

    private List<PinObject> _pins = new List<PinObject>();
    private PinObject _activePin;
    private PinUI _pinUIInstance;
    private PinContextUI _contextUIInstance;
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

    public void InitializeUI() {
        _sceneToInstantiate = SceneLoadingManager.Instance.GetActiveScene(); // Just to be sure...
        _pinUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(PinUIPrefab).GetComponent<PinUI>();
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
        _pinUIInstance.UpdatePinButtonList();

        if (!deserialization) MarkUnsavedChanges();
    }

    public void DeletePin(PinObject pin) {
        PopUp.Instance.AreYouSurePopUp((sure) => {
            if (!sure) return;

            if (pin == _activePin) CloseContextUI();
            _pins.Remove(pin);
            Destroy(pin.gameObject);
            _pinUIInstance.UpdatePinButtonList();
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

        if (_contextUIInstance == null)
            _contextUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(PinContextUIPrefab, _sceneToInstantiate).GetComponent<PinContextUI>();
        _contextUIInstance.Initialize(pin, this);
        ToggleContextUI(true);

        pin.SetOutline(true);
        _pinUIInstance.UpdatePinButtonList();
    }

    public void CloseContextUI() {
        if (_activePin != null) _activePin.SetOutline(false);
        _activePin = null;

        ToggleContextUI(false);
        _pinUIInstance.OnExitEditing();
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

    public void ToggleContextUI(bool show) {
        if (_contextUIInstance == null) return;
        _contextUIInstance.gameObject.SetActive(show);
    }

    public void RefreshPinButtons() {
        _pinUIInstance.UpdatePinButtonList();
    }

    #endregion

    public void ClearEverything() {
        _placingManager.CancelPlacing();
        CloseContextUI();
        Utilities.DestroyAllGameObjects(_pins);
        _pinUIInstance.ClearPinButtonList();
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
