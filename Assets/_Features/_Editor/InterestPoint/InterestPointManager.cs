using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestPointManager : Singleton<InterestPointManager> {
    
    public GameObject InterestPointPrefab;
    List<InterestPoint> interestPoints = new List<InterestPoint>();
    public Vector3 interestPointSpawnPosition;
    public GameObject cameraViewUI;
    public Camera previewCam;

    InterestPoint currentInterestPoint;
    public bool isActivelyShowingCam = false;

    protected override void Awake() {
        base.Awake();

        ToggleCameraPreview(false);
    }

    public GameObject CreateNewInterestPoint() {
        InterestPoint newInterestPoint = SceneLoadingManager.Instance.InstantiateObjectInScene(InterestPointPrefab, interestPointSpawnPosition, SceneType.Editing).GetComponent<InterestPoint>();
        interestPoints.Add(newInterestPoint);
        newInterestPoint.Deactivate();
        return newInterestPoint.gameObject;
    }

    public void SetActiveInterestPoint(InterestPoint ip) {
        currentInterestPoint = ip;
        ToggleCameraPreview(true);
    }

    public void ActivateInterestPoint() {
        isActivelyShowingCam = true;
        currentInterestPoint?.Activate();
    }

    public void DeactivateInterestPoint() {
        isActivelyShowingCam = false;
        currentInterestPoint?.Deactivate();
    }

    public List<InterestPoint> GetInterestPoints() {
        return interestPoints;
    }
    public void ClearEverything() {
        DeactivateInterestPoint();
        SetActiveInterestPoint(null);
        Utilities.DestroyAllGameObjects(interestPoints);
    }

    void ToggleCameraPreview(bool toggleOn) {
        if(cameraViewUI == null || previewCam == null || currentInterestPoint == null) return;

        if (toggleOn) {
            // Move the camera to the current vcam - must be continous so child
            previewCam.transform.SetParent(currentInterestPoint.gameObject.transform);
            previewCam.transform.localPosition = new Vector3(0, 0, 0);
            previewCam.transform.rotation = currentInterestPoint.transform.rotation;
        }
        // Toggle UI and cam
        cameraViewUI.SetActive(toggleOn);
        previewCam.gameObject.SetActive(toggleOn);
    }
    public SerializableInterestPointManager Serialize() { 
        List<SerializableInterestPoint> serializablePoints = new List<SerializableInterestPoint>();
        foreach (var interestPoint in interestPoints) {
            SerializableInterestPoint instantiated = interestPoint.Serialize();
            serializablePoints.Add(instantiated);
        }

        SerializableInterestPointManager serializedManager = new SerializableInterestPointManager {
            interestPoints = serializablePoints
        };
        return serializedManager;
    }
    public void Deserialize(SerializableInterestPointManager serializedManager) {
        foreach (var serializedInterestPoint in serializedManager.interestPoints) {
            InterestPoint iPoint = CreateNewInterestPoint().GetComponent<InterestPoint>();
            iPoint.Deserialize(serializedInterestPoint);
        }
    }

}

[Serializable]
public class SerializableInterestPointManager {
    public List<SerializableInterestPoint> interestPoints;
}