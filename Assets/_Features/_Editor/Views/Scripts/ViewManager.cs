using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ViewManager : MonoBehaviour {

    public SceneType SceneToInstantiate = SceneType.Editing;
    public GameObject ViewPointPrefab;
    List<ViewPoint> viewPoints = new List<ViewPoint>();
    //   List<EditorObjectBase> interestPoints = new List<EditorObjectBase>();
    public Vector3 viewPointSpawnPosition;
    public GameObject cameraViewUI;
    public Camera previewCam;

    ViewPoint currentViewPoint;
    CustomMovement _movementScript;
    public bool isActivelyShowingCam = false;

    [System.Serializable]
    public class OnViewAdded : UnityEvent<ViewPoint> { }
    public OnViewAdded OnViewPointAddedEvent;

    void Awake() {
        _movementScript = GetComponent<CustomMovement>();

        print("AWAKE - IS PREFAB NULL? " + ViewPointPrefab == null);
        if (ViewPointPrefab == null) {
            ViewPointPrefab = Resources.Load<GameObject>("ViewCamGameObject");
            print("AWAKE - IS PREFAB still null after resources? " + ViewPointPrefab == null);
        }
    }

    void OnEnable() {
        ToggleCameraPreview(false);
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

    public GameObject CreateNewViewPoint() {
        // 1. Check Manager Instance
        if (SceneLoadingManager.Instance == null) {
            Debug.LogError("DEBUG: SceneLoadingManager.Instance is NULL!");
            return null;
        }

        // 2. Check Prefab Assignment
        if (ViewPointPrefab == null) {
            Debug.LogError("DEBUG: ViewPointPrefab is NOT ASSIGNED in the inspector!");
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

        print($"DEBUG: Instantiating {ViewPointPrefab.name} at {spawnPos}");

        GameObject spawnedObj = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointPrefab, spawnPos, SceneToInstantiate);

        if (spawnedObj == null) {
            Debug.LogError("DEBUG: SceneLoadingManager failed to instantiate object!");
            return null;
        }

        ViewPoint newInterestPoint = spawnedObj.GetComponent<ViewPoint>();
        if (newInterestPoint == null) {
            Debug.LogError("DEBUG: Spawned object is missing ViewPoint script!");
            return spawnedObj; // Return anyway so we don't crash, but error is logged
        }

        newInterestPoint.SetName("Default view point name" + new System.Random().Next(0, 100));
        newInterestPoint.transform.rotation = spawnRot;
        newInterestPoint.Deactivate();

        viewPoints.Add(newInterestPoint);

        if (OnViewPointAddedEvent != null) {
            OnViewPointAddedEvent.Invoke(newInterestPoint);
        } else {
            Debug.LogWarning("DEBUG: OnViewPointAddedEvent is null");
        }

        return newInterestPoint.gameObject;
    }

    public void SetActiveViewPoint(ViewPoint ip) {
        currentViewPoint = ip;
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
        FindAnyObjectByType<ViewPointUI>().ClearViewButtonList();
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
            Debug.LogError("Interest point manager deserialization failed: Source data is null");
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

        print("TRYING TO DESERIALIZE THE VIEWS");
        var ui = FindAnyObjectByType<ViewPointUI>();
        if (ui != null) {
            ui.UpdateViewButtonList();
        } else {
            Debug.LogWarning("DEBUG: ViewPointUI not found in scene.");
        }
    }

}

[Serializable]
public class SerializableViewPointManager {
    public List<SerializableViewPoint> ViewPoints;
}