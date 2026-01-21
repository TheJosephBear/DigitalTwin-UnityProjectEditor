using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;
// using FrostweepGames.Plugins.WebGLFileBrowser;

public class MapUI : UIBehaviour {

    public void onX() {
        UIManager.Instance.HideUI(UIType.MapUI);
    }

    public void onNahrat() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMap, "obj, mtl, png, jpg, jpeg", true);
    }

    public void onPridatVariantu() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMapVar, "obj, mtl, png, jpg, jpeg", true);
    }

    void OnFileSelectedMap(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.SetBaseMapModel(AssetManager.Instance.CreateNewAssetFromFiles(files));
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }

    void OnFileSelectedMapVar(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.UploadMapVariant(AssetManager.Instance.CreateNewAssetFromFiles(files));
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }
    }
    
}