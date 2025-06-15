using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitUI : MonoBehaviour
{
    public void OnMapUpload() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMap);
    }

    public void OnExit() {
        EditorManager.Instance.ExitEditor();
    }

    void OnFileSelectedMap(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            MapManager.Instance.UploadBaseMapModel(AssetManager.Instance.CreateNewAsset(files[0]));
            // Open Geo localization
            EditorManager.Instance.ChangeEditorMode(EditorMode.GeoLocalization);
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }


}
