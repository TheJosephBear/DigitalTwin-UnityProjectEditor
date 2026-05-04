using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeoLocalizationUI : UIBehaviour {

    public TextMeshProUGUI InfoTextReff;
    public TMP_InputField ZoomScaleInputFieldReff;
    public Movable Basemapreff;

    public void PrintInfo(string text) {
        InfoTextReff.text = text;
    }

    public void ZoomToScale() {
        float zoomScale;
        if (float.TryParse(ZoomScaleInputFieldReff.text, out zoomScale)) {
            EditorManager.Instance.GeoMapManager.ZoomToFitScale(zoomScale, 1f);
        }
    }

    public void OnZoomSliderUpdate(float value) {
        GeoMapLocalizationManager.Instance.ZoomMap(value);
    }

    public void OnLockGeoMap() {
        GeoMapLocalizationManager.Instance.ToggleLock();
    }

    public void OnPlaceMapModel() {
        GeoMapLocalizationManager.Instance.PlaceMapModel();
    }

    public void OnExit() {
        EditorManager.Instance.GeoMapManager.ExitGeoLocalization();
    }

    public void OnExitToMenu() {
        EditorManager.Instance.GeoMapManager.LeaveToMenu();
        UIManager.Instance.HideUI(UIType.GeoLocalizationUI);
    }

    public void OnUploadMap() {
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            EditorManager.Instance.MapManager.SetBaseMapModel(createdAsset);
            GeoMapLocalizationManager.Instance.Setup();
        });
    }
}
