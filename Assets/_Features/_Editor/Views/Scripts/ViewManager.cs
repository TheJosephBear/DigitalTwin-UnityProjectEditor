using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ViewManager : MonoBehaviour {

    public SceneType SceneToInstantiate = SceneType.Editing;
    public bool ShowAddViewButton = false;
    public GameObject ViewPointPrefab;
    public GameObject ViewPointUIPrefab;
    List<ViewPoint> viewPoints = new List<ViewPoint>();
    //   List<EditorObjectBase> interestPoints = new List<EditorObjectBase>();
    public Vector3 viewPointSpawnPosition;
    public GameObject cameraViewUI;
    public Camera previewCam;

    ViewPointUI _viewPointUIInstance;
    ViewPoint currentViewPoint;
    CustomMovement _movementScript;
    public bool isActivelyShowingCam = false;

    [System.Serializable]
    public class OnViewAdded : UnityEvent<ViewPoint> { }
    public OnViewAdded OnViewPointAddedEvent;

    void Awake() {
        _movementScript = GetComponent<CustomMovement>();
    }

    void OnEnable() {
        if (_viewPointUIInstance == null && SceneLoadingManager.Instance != null) InitializeUI();
        ToggleCameraPreview(false);
    }

    void InitializeUI() {
        _viewPointUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointUIPrefab, SceneToInstantiate).GetComponent<ViewPointUI>();
        _viewPointUIInstance.Initialize(this, ShowAddViewButton);
    }

    public void ToggleViewPointUI(bool show) {
        _viewPointUIInstance.gameObject.SetActive(show);
    }

    public void StartViewMoving() {
        ActivateViewPoint();
        // Start controlling it
        _movementScript.SetTarget(currentViewPoint.gameObject);
    }

    public void ExitViewMoving() {
        DeactivateViewPoint();
        _movementScript.SetTarget(null);
    }

    public GameObject CreateNewViewPoint(bool updateUI = true) {
        if (SceneLoadingManager.Instance == null) {
            Debug.LogError("DEBUG: SceneLoadingManager.Instance is NULL!");
            return null;
        }

        if (ViewPointPrefab == null) {
            return null;
        }

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        if (EditorManager.Instance?.EditorCameraManager != null) {
            var freecam = EditorManager.Instance.EditorCameraManager.GetFreeCamTransform();
            if (freecam != null) {
                spawnPos = freecam.position;
                spawnRot = freecam.rotation;
            }
        }

        GameObject spawnedObj = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointPrefab, spawnPos, SceneToInstantiate);
        if (spawnedObj == null) {
            return null;
        }

        ViewPoint newInterestPoint = spawnedObj.GetComponent<ViewPoint>();
        if (newInterestPoint == null) {
            return spawnedObj; // Return anyway so we don't crash, but error is logged
        }

    //    newInterestPoint.SetName("Default view point name " + newInterestPoint.ID);
        newInterestPoint.SetName("Pohled " + (viewPoints.Count+1).ToString());
        newInterestPoint.transform.rotation = spawnRot;
        newInterestPoint.Deactivate();

        viewPoints.Add(newInterestPoint);
        if (updateUI) _viewPointUIInstance.UpdateViewButtonList();

        if (OnViewPointAddedEvent != null) {
            OnViewPointAddedEvent.Invoke(newInterestPoint);
        } else {
            Debug.LogWarning("OnViewPointAddedEvent is null");
        }

        return newInterestPoint.gameObject;
    }

    // Clicking the specific view button
    public void OnViewHUDButton(ViewPoint viewPoint) {

    }

    public void SetActiveViewPoint(ViewPoint vp) {
        currentViewPoint = vp;
        ToggleCameraPreview(true);
    }

    public void ActivateViewPoint() {
        isActivelyShowingCam = true;
        currentViewPoint?.Activate();
    }

    public void DeactivateViewPoint() {
        isActivelyShowingCam = false;
        currentViewPoint?.Deactivate();
    }

    public List<ViewPoint> GetViewPoints() {
        return viewPoints;
    }

    public void ClearEverything() {
        ExitViewMoving();
        SetActiveViewPoint(null);
        Utilities.DestroyAllGameObjects(viewPoints);
        _viewPointUIInstance.ClearViewButtonList();
    }

    public void ToggleCameraPreview(bool toggleOn) {
        if (cameraViewUI == null || previewCam == null) return;

        if (toggleOn) {
            if (currentViewPoint == null) return;
            // Move the camera to the current vcam - must be continous so child
            previewCam.transform.SetParent(currentViewPoint.gameObject.transform);
            previewCam.transform.localPosition = new Vector3(0, 0, 0);
            previewCam.transform.rotation = currentViewPoint.transform.rotation;
        }
        // Toggle UI and cam
        cameraViewUI.SetActive(toggleOn);
        previewCam.gameObject.SetActive(toggleOn);
    }

    public ViewPoint GetViewPointByID(string id) {
        return viewPoints.Find(vp => vp.ID == id);
    }

    public List<SerializableViewPoint> GetSerializedViewPointsList() {
        List<SerializableViewPoint> list = new List<SerializableViewPoint>();

        foreach (ViewPoint vp in viewPoints) {
            list.Add(vp.Serialize());
        }

        return list;
    }

    public SerializableViewPointManager Serialize() {
        List<SerializableViewPoint> serializablePoints = new List<SerializableViewPoint>();
        foreach (var interestPoint in viewPoints) {
            SerializableViewPoint instantiated = interestPoint.Serialize();
            serializablePoints.Add(instantiated);
        }

        SerializableViewPointManager serializedManager = new SerializableViewPointManager {
            ViewPoints = serializablePoints
        };
        return serializedManager;
    }

    public void Deserialize(SerializableViewPointManager serializedManager) {
        if (serializedManager == null || serializedManager.ViewPoints == null) {
            Debug.Log("Interest point manager deserialization failed: Source data is null");
            return;
        }

        foreach (var serializedInterestPoint in serializedManager.ViewPoints) {
            print("DEBUG: Starting loop for a ViewPoint");

            GameObject vpObject = CreateNewViewPoint();

            if (vpObject == null) {
                Debug.LogError("DEBUG: CreateNewViewPoint returned NULL!");
                continue;
            }

            ViewPoint iPoint = vpObject.GetComponent<ViewPoint>();
            if (iPoint == null) {
                Debug.LogError("DEBUG: ViewPoint component missing on the instantiated object!");
                continue;
            }

            print("DEBUG: Calling iPoint.Deserialize now...");
            iPoint.Deserialize(serializedInterestPoint);
        }
    }

}

[Serializable]
public class SerializableViewPointManager {
    public List<SerializableViewPoint> ViewPoints;
}