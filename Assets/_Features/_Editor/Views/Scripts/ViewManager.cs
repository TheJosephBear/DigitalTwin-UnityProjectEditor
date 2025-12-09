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
        Transform freecamTrans = EditorManager.Instance.EditorCameraManager?.GetFreeCamTransform(); // Dependability..
        if(freecamTrans != null)
            viewPointSpawnPosition = freecamTrans.position;

        ViewPoint newInterestPoint = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointPrefab, viewPointSpawnPosition, SceneToInstantiate).GetComponent<ViewPoint>();
        newInterestPoint.SetName("Default view point name" + new System.Random().Next(0,100) );
        viewPoints.Add(newInterestPoint);
        if (freecamTrans != null)
            newInterestPoint.transform.rotation = freecamTrans.rotation;
        newInterestPoint.Deactivate();

        OnViewPointAddedEvent.Invoke(newInterestPoint);
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
        if(cameraViewUI == null || previewCam == null) return;

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
            print("Interest point manager deserialization failed");
            return;
        }

        foreach (var serializedInterestPoint in serializedManager.ViewPoints) {
            ViewPoint iPoint = CreateNewViewPoint().GetComponent<ViewPoint>();
            iPoint.Deserialize(serializedInterestPoint);
        }
        
        FindAnyObjectByType<ViewPointUI>().UpdateViewButtonList(); // Handle differently in the future pls
    }

}

[Serializable]
public class SerializableViewPointManager {
    public List<SerializableViewPoint> ViewPoints;
}