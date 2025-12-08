using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;
using FrostweepGames.Plugins.WebGLFileBrowser;

public class MapUI : UIBehaviour {


    public void onX() {
        UImanager.Instance.HideUI(UIType.MapUI);
    }

    public void onNahrat() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMap);
    }

    public void onPridatVariantu() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMapVar);
    }


    void OnFileSelectedMap(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.SetBaseMapModel(AssetManager.Instance.CreateNewAsset(files[0]));
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }

    void OnFileSelectedMapVar(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.UploadMapVariant(AssetManager.Instance.CreateNewAsset(files[0]));
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }
    }

}