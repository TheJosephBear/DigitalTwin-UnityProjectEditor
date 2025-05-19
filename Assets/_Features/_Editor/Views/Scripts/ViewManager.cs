using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : Singleton<ViewManager> {

    public SceneType SceneToInstantiate = SceneType.Editing;
    public GameObject ViewPointPrefab;
    List<ViewPoint> viewPoints = new List<ViewPoint>();
 //   List<EditorObjectBase> interestPoints = new List<EditorObjectBase>();
    public Vector3 viewPointSpawnPosition;
    public GameObject cameraViewUI;
    public Camera previewCam;

    ViewPoint currentViewPoint;
    public bool isActivelyShowingCam = false;

    protected override void Awake() {
        base.Awake();
    }

    void OnEnable() {
        ToggleCameraPreview(false);
    }

    public GameObject CreateNewViewPoint() {
        ViewPoint newInterestPoint = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointPrefab, viewPointSpawnPosition, SceneToInstantiate).GetComponent<ViewPoint>();
        viewPoints.Add(newInterestPoint);
        newInterestPoint.Deactivate();
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
        DeactivateViewPoint();
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

    public void FillEditorObjectUI() {
    //    print("velikost pøed: "+interestPoints.Count);
        List<EditorObjectBase> abstractList = new List<EditorObjectBase>();
        foreach (ViewPoint point in viewPoints) {
            EditorObjectBase abstractReff = point;
            abstractList.Add(point);
        }
      //  EditorObjectManager.Instance.FillEditorObjectListUI(interestPoints, "Kamery");
        EditorObjectManager.Instance.FillEditorObjectListUI(abstractList, "Kamery");
    }

    public SerializableInterestPointManager Serialize() { 
        List<SerializableInterestPoint> serializablePoints = new List<SerializableInterestPoint>();
        foreach (var interestPoint in viewPoints) {
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
            ViewPoint iPoint = CreateNewViewPoint().GetComponent<ViewPoint>();
            iPoint.Deserialize(serializedInterestPoint);
        }
    }

}

[Serializable]
public class SerializableInterestPointManager {
    public List<SerializableInterestPoint> interestPoints;
}