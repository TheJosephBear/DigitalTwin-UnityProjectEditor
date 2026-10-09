using System;
using System.Collections.Generic;
using UnityEngine;

public class PinManager: Singleton<PinManager> {
    [Header("Prefabs & Setup")]
    public SceneType SceneToInstantiate = SceneType.Editing;
    public GameObject PinPrefab;
    public GameObject PinUIPrefab;
    public GameObject PinPopUpPrefab;

    private List<PinObject> pins = new List<PinObject>();
    private PinObject activePin;
    private PinUI pinUIInstance;
    private PinContextUI popUpInstance;
 //   private PinMovingManager movingManager;

    protected override void Awake() {
        base.Awake();
   //     movingManager = GetComponent<PinMovingManager>();
    }

    private void OnEnable() {
        if (pinUIInstance == null && SceneLoadingManager.Instance != null) {
            InitializeUI();
        }
    }

    private void InitializeUI() {
        GameObject uiObj = SceneLoadingManager.Instance.InstantiateObjectInScene(PinUIPrefab, SceneToInstantiate);
        pinUIInstance = uiObj.GetComponent<PinUI>();
        pinUIInstance.Initialize(this);
    }

    public PinObject CreateNewPin() {
        Vector3 spawnPos = Vector3.zero;

        if (CameraManager.Instance != null) {
            var freecam = CameraManager.Instance.GetFreeCamTransform();
            if (freecam != null) {
                spawnPos = freecam.position + freecam.forward * 2f;
            }
        }

        GameObject spawnedObj = SceneLoadingManager.Instance.InstantiateObjectInScene(PinPrefab, spawnPos, SceneToInstantiate);
        PinObject newPin = spawnedObj.GetComponent<PinObject>();

        newPin.Name = "Pin " + (pins.Count + 1);
        pins.Add(newPin);

        pinUIInstance.UpdatePinList(pins);
        SelectPin(newPin);

        return newPin;
    }

    public void SelectPin(PinObject pin) {
        activePin = pin;

        // Show UI details panel
        if (pinUIInstance != null) {
            pinUIInstance.ShowDetails(pin);
        }

        // Show attached world popup
        ShowPopUpForPin(pin);
    }

    private void ShowPopUpForPin(PinObject pin) {
        if (popUpInstance == null && PinPopUpPrefab != null) {
            GameObject popUpObj = SceneLoadingManager.Instance.InstantiateObjectInScene(PinPopUpPrefab, SceneToInstantiate);
            popUpInstance = popUpObj.GetComponent<PinContextUI>();
        }

        if (popUpInstance != null) {
            popUpInstance.gameObject.SetActive(true);
            popUpInstance.Initialize(pin);
        }
    }

    public void HidePopUp() {
        if (popUpInstance != null) {
            popUpInstance.gameObject.SetActive(false);
        }
    }

    public void StartMovingPin(PinObject pin) {

    }

    public List<PinObject> GetPins() => pins;

    #region Serialization

    public SerializablePinManager Serialize() {
        List<SerializablePin> list = new List<SerializablePin>();
        foreach (PinObject pin in pins) {
            list.Add(pin.Serialize());
        }
        return new SerializablePinManager { Pins = list };
    }

    public void Deserialize(SerializablePinManager data) {
        if (data == null || data.Pins == null) return;

        foreach (var pinData in data.Pins) {
            PinObject newPin = CreateNewPin();
            newPin.Deserialize(pinData);
        }

        pinUIInstance.UpdatePinList(pins);
    }

    #endregion
}

[Serializable]
public class SerializablePin {
    public string ID;
    public string Name;
    public string Description;
    public Vector3 Position;
    // Add sprite reference ID or path here if loaded dynamically
}

[Serializable]
public class SerializablePinManager {
    public System.Collections.Generic.List<SerializablePin> Pins;
}
