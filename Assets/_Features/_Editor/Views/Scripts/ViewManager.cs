using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ViewManager : Singleton<ViewManager> {

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
    ViewPoint _activeViewPoint;
    ViewMovingManager _viewMovingScript;
    public bool isActivelyShowingCam = false;

    [System.Serializable]
    public class OnViewAdded : UnityEvent<ViewPoint> { }
    public OnViewAdded OnViewPointAddedEvent;

    protected override void Awake() {
        base.Awake();
        _viewMovingScript = GetComponent<ViewMovingManager>();
    }

    void OnEnable() {
        if (_viewPointUIInstance == null && SceneLoadingManager.Instance != null) InitializeUI();
        ToggleCameraPreview(false);
    }

    void InitializeUI() {
        _viewPointUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointUIPrefab, SceneToInstantiate).GetComponent<ViewPointUI>();
        _viewPointUIInstance.Initialize(this, ShowAddViewButton);
    }

    #region View moving

    public void StartViewMoving() {
        ActivateViewPoint();
        ToggleViewPointUI(true);
        _viewMovingScript.StartViewMoving(_activeViewPoint);
    }

    public void ExitViewMoving(bool save) {
        _viewMovingScript.ExitViewMoving(save, (success) => {
            if (!success) return;

            MoveMainCamToActiveViewPoint();
            DeactivateViewPoint();
            _viewPointUIInstance.UpdateViewButtonList();
            _viewPointUIInstance.ResetButtonsVisual();
            MainManagerBase.Instance.ChangeState(AppState.Freecam);
        });
    }

    #endregion

    #region View point management

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
        newInterestPoint.SetName("Pohled " + (viewPoints.Count + 1).ToString());
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

    public void DeleteViewPoint(ViewPoint viewPoint) {
        PopUp.Instance.AreYouSurePopUp((sure) => {
            if (sure) {
                if(viewPoint == _activeViewPoint) {
                    _activeViewPoint = null;
                }
                viewPoints.Remove(viewPoint);
                Destroy(viewPoint.gameObject);
                _viewPointUIInstance.UpdateViewButtonList();
            }
        }, $"Chcete smazat pohled {viewPoint.Name}?");
    }

    public void SetActiveViewPoint(ViewPoint vp) {
        DeactivateViewPoint();
        _activeViewPoint = vp;
        //    ToggleCameraPreview(true);
    }

    public ViewPoint GetActiveViewPoint() {
        return _activeViewPoint;
    }

    public List<ViewPoint> GetViewPoints() {
        return viewPoints;
    }

    public ViewPoint GetViewPointByID(string id) {
        return viewPoints.Find(vp => vp.ID == id);
    }

    #endregion

    #region UI management

    public void ToggleCameraPreview(bool toggleOn) {
        if (cameraViewUI == null || previewCam == null) return;

        if (toggleOn) {
            if (_activeViewPoint == null) return;
            // Move the camera to the current vcam - must be continous so child
            previewCam.transform.SetParent(_activeViewPoint.gameObject.transform);
            previewCam.transform.localPosition = new Vector3(0, 0, 0);
            previewCam.transform.rotation = _activeViewPoint.transform.rotation;
        }
        // Toggle UI and cam
        cameraViewUI.SetActive(toggleOn);
        previewCam.gameObject.SetActive(toggleOn);
    }

    public void ToggleViewPointUI(bool show) {
        _viewPointUIInstance.gameObject.SetActive(show);
    }

    #endregion

    public void MoveMainCamToActiveViewPoint() {
        GameObject freeCam = MainManagerBase.Instance.EditorCameraManager.GetFreeCamVcam();
        freeCam.transform.position = _activeViewPoint.transform.position;
        freeCam.transform.rotation = _activeViewPoint.transform.rotation;
    }

    public void ActivateViewPoint() {
        if (_activeViewPoint == null) return;
        isActivelyShowingCam = true;
        _activeViewPoint?.Activate();
    }

    public void DeactivateViewPoint() {
        if (_activeViewPoint == null) return;
        isActivelyShowingCam = false;
        _activeViewPoint?.Deactivate();
    }

    public void ClearEverything() {
        ExitViewMoving(false);
        SetActiveViewPoint(null);
        Utilities.DestroyAllGameObjects(viewPoints);
        _viewPointUIInstance.ClearViewButtonList();
    }

    #region Serialization

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

            GameObject vpObject = CreateNewViewPoint();

            if (vpObject == null) {
                continue;
            }

            ViewPoint iPoint = vpObject.GetComponent<ViewPoint>();
            if (iPoint == null) {
                continue;
            }

            iPoint.Deserialize(serializedInterestPoint);
        }
    }

    #endregion

}

[Serializable]
public class SerializableViewPointManager {
    public List<SerializableViewPoint> ViewPoints;
}