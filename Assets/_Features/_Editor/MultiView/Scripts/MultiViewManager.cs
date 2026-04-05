using System.Collections.Generic;
using UnityEngine;

public class MultiViewManager : MonoBehaviour {

    public GameObject MultiviewCamerasPrefab;

    private Transform _multiviewCamerasSpawnTransform;
    private MultiviewUI _multiviewUIRefference;
    private GameObject _multivewCamerasRefference;
    private MapVariant _primaryMapInstance;
    private MapVariant _secondaryMapInstance;

    public void EnterMultiView() {
        _multiviewCamerasSpawnTransform = EditorManager.Instance.EditorCameraManager.GetFreeCamTransform();

        if (_multiviewUIRefference == null) {
            _multiviewUIRefference = FindAnyObjectByType<MultiviewUI>();
        }

        if(_multivewCamerasRefference == null) {
            _multivewCamerasRefference = SceneLoadingManager.Instance.InstantiateObjectInScene(MultiviewCamerasPrefab, _multiviewCamerasSpawnTransform.position, SceneType.Editing);
        } else {
            _multivewCamerasRefference.SetActive(true);
        }

        // Update camera position
        _multivewCamerasRefference.transform.position = _multiviewCamerasSpawnTransform.position;
        _multivewCamerasRefference.transform.rotation = _multiviewCamerasSpawnTransform.rotation;

        UIManager.Instance.ShowUI(UIType.TwoMapsCameraView);
    }

    public void Exit() {
        _primaryMapInstance.gameObject.SetActive(false);
        _secondaryMapInstance.gameObject.SetActive(false);
        _multivewCamerasRefference.SetActive(false);
        UIManager.Instance.HideUI(UIType.TwoMapsCameraView);
        EditorManager.Instance.ChangeState(AppState.Freecam);
    }



    public void ShowVariant(MapVariant originalVariant, MapPriority priority) {
        if (originalVariant == null) {
            Debug.LogWarning("Cannot display null map variant.");
            return;
        }

        switch (priority) {
            case MapPriority.Primary:
                if (_primaryMapInstance != null)
                    Destroy(_primaryMapInstance.gameObject);
                break;

            case MapPriority.Secondary:
                if (_secondaryMapInstance != null)
                    Destroy(_secondaryMapInstance.gameObject);
                break;
        }

        GameObject cloneGO = SceneLoadingManager.Instance.InstantiateObjectInScene(originalVariant.gameObject, EditorManager.Instance.MapManager.mapSpawnPosition, SceneType.Editing);  

        MapVariant clone = cloneGO.AddComponent<MapVariant>();
        clone.ModelAsset = originalVariant.ModelAsset;
        clone.ToggleMeshVisibility(true);
        clone.SetMeshLayer(priority);

        switch (priority) {
            case MapPriority.Primary:
                _primaryMapInstance = clone;
                break;

            case MapPriority.Secondary:
                _secondaryMapInstance = clone;
                break;
        }
    }

    public void LockMap(MapPriority priority, bool isLocked) {
        switch (priority) {
            case MapPriority.Primary:
                if (_primaryMapInstance != null)
                    _primaryMapInstance.IsLocked = isLocked;
                break;

            case MapPriority.Secondary:
                if (_secondaryMapInstance != null)
                    _secondaryMapInstance.IsLocked = isLocked;
                break;
        }
    }

    public void ClearDisplayedMaps() {
        if (_primaryMapInstance != null)
            Destroy(_primaryMapInstance.gameObject);
        if (_secondaryMapInstance != null)
            Destroy(_secondaryMapInstance.gameObject);

        _primaryMapInstance = null;
        _secondaryMapInstance = null;
    }
}
