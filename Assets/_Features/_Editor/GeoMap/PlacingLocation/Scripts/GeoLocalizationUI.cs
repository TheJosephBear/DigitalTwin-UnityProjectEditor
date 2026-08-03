using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class GeoLocalizationUI : UIBehaviour {

    public GameObject ExitToMenuButton;
    public TextMeshProUGUI InfoTextReff;
    public TMP_InputField ZoomScaleInputFieldReff;
    public Movable Basemapreff; 
    public UISwitcher.UISwitcher LockToggleRef;

    bool _firstOpen = false;


    #region OnClick

    public void OnUploadMap() {
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            GeoMapLocalizationManager.Instance.UploadMap(createdAsset);
        });
        /*
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            EditorManager.Instance.MapManager.SetBaseMapModel(createdAsset);
            GeoMapLocalizationManager.Instance.Setup();
        });
        */
    }

    public void OnLockGeoMap() {
        GeoMapLocalizationManager.Instance.ToggleLock();
    }

    public void OnPlaceMapModel() {
        GeoMapLocalizationManager.Instance.PlaceMapModel();
    }

    public void OnExit() {
        if (_firstOpen) {
            EditorManager.Instance.GeoMapManager.LeaveToMenu((exitSuccess) => {
                if (exitSuccess) UIManager.Instance.HideUI(UIType.GeoLocalizationUI);
            });
        } else {
            EditorManager.Instance.GeoMapManager.ExitGeoLocalization();
        }
    }

    public void OnLock(bool toggleValue) {
        GeoMapLocalizationManager.Instance.ToggleLock(toggleValue);
    }

    public void OnZoomSliderUpdate(float value) {
        GeoMapLocalizationManager.Instance.ZoomMap(value);
    }

    public void OnOpacitySliderUpdate(float value) {
        GeoMapLocalizationManager.Instance.UpdateCloneOpacity(value);
    }


    #endregion

    public void Initialize(bool firstOpen) {
        _firstOpen = firstOpen;
    }

    public void ToggleExitToMenuButtonVisibility(bool toggleOn) {
        ExitToMenuButton.SetActive(toggleOn);
    }

    public void ChangeLockVisual(bool toggleOn) {
        LockToggleRef.SetWithoutNotify(toggleOn);
    }

    public void PrintInfo(string text) {
        InfoTextReff.text = text;
    }

    public void ZoomToScale() {
        float zoomScale;
        if (float.TryParse(ZoomScaleInputFieldReff.text, out zoomScale)) {
            EditorManager.Instance.GeoMapManager.ZoomToFitScale(zoomScale, 1f);
        }
    }
}
