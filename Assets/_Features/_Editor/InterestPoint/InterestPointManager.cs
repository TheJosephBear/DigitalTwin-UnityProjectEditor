using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterestPointManager : Singleton<InterestPointManager> {
    
    public GameObject InterestPointPrefab;
    List<InterestPoint> interestPoints = new List<InterestPoint>();
 //   List<EditorObjectBase> interestPoints = new List<EditorObjectBase>();
    public Vector3 interestPointSpawnPosition;
    public GameObject cameraViewUI;
    public Camera previewCam;

    InterestPoint currentInterestPoint;
    public bool isActivelyShowingCam = false;

    protected override void Awake() {
        base.Awake();
    }

    void OnEnable() {
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

    public void ToggleCameraPreview(bool toggleOn) {
        if(cameraViewUI == null || previewCam == null) return;

        if (toggleOn) {
            if (currentInterestPoint == null) return;
            // Move the camera to the current vcam - must be continous so child
            previewCam.transform.SetParent(currentInterestPoint.gameObject.transform);
            previewCam.transform.localPosition = new Vector3(0, 0, 0);
            previewCam.transform.rotation = currentInterestPoint.transform.rotation;
        }
        // Toggle UI and cam
        cameraViewUI.SetActive(toggleOn);
        previewCam.gameObject.SetActive(toggleOn);
    }

    public void FillEditorObjectUI() {
    //    print("velikost pøed: "+interestPoints.Count);
        List<EditorObjectBase> abstractList = new List<EditorObjectBase>();
        foreach (InterestPoint point in interestPoints) {
            EditorObjectBase abstractReff = point;
            abstractList.Add(point);
        }
      //  EditorObjectManager.Instance.FillEditorObjectListUI(interestPoints, "Kamery");
        EditorObjectManager.Instance.FillEditorObjectListUI(abstractList, "Kamery");
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
        if (serializedManager == null || serializedManager.interestPoints == null) {
            print("Interest point manager deserialization failed");
            return;
        }

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