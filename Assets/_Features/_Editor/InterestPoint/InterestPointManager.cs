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

    public void CreateNewInterestPoint() {
        InterestPoint newInterestPoint = SceneLoadingManager.Instance.InstantiateObjectInScene(InterestPointPrefab, interestPointSpawnPosition, SceneType.Editing).GetComponent<InterestPoint>();
        interestPoints.Add(newInterestPoint);
        newInterestPoint.Deactivate();
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

    void ToggleCameraPreview(bool toggleOn) {
        if (toggleOn) {
            // Move the camera to the current vcam - must be continous so child
            previewCam.transform.SetParent(currentInterestPoint.gameObject.transform);
            previewCam.transform.localPosition = new Vector3(0, 0, 0);
        }
        // Toggle UI and cam
        cameraViewUI.SetActive(toggleOn);
        previewCam.gameObject.SetActive(toggleOn);
    }


    /*
    public void UploadBaseMapModel(ModelAsset newMap) {
        baseMap = newMap;
        SpawnMap();
    }

    public void UploadMapVariant(ModelAsset newMap) {
        mapVariants.Add(newMap);
    }

    public void SpawnMap() {
        GameObject go = baseMap?.InstantiateModel(mapSpawnPosition);
        go?.SetActive(true);
        /*
        if (go != null) spinniiiieeee.Add(go);
        if (go != null) go.transform.Rotate(new Vector3(-90, 0, 0));
        *//*
    }


    public void SpawnSelectedVariant(int index) {
        if (currentMapVarInstance != null) {
            Destroy(currentMapVarInstance);
            currentMapVarInstance = null;
        }
        if (index >= 0 && index < mapVariants.Count) {
            currentMapVarInstance = mapVariants[index]?.InstantiateModel(mapSpawnPosition);
            currentMapVarInstance?.SetActive(true);
            AddLayerToAllChildren(currentMapVarInstance);
        }
    }


    void AddLayerToAllChildren(GameObject g) {
        foreach (Transform child in g.GetComponentsInChildren<Transform>()) {
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                if (child.GetComponent<MeshCollider>() == null) {
                    child.gameObject.layer = LayerMask.NameToLayer("SecondaryMap");
                }
            }
        }
    }

    public void ClearEverything() {
        baseMap = null;
    }

    public bool hasVariant() {
        return mapVariants.Count > 0;
    }

    public List<ModelAsset> GetVariants() {
        return mapVariants;
    }
*/
}
