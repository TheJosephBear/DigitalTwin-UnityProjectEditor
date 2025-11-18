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

    public void OnUploadMap() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMap);
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

    void OnFileSelectedMap(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.UploadBaseMapModel(AssetManager.Instance.CreateNewAsset(files[0]));
            GeoMapLocalizationManager.Instance.Setup();
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }

}
