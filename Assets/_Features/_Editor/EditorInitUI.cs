using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitUI : UIBehaviour {
    public void OnMapUpload() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMap);
    }

    public void OnExit() {
        if (MainManagerBase.Instance is EditorManager editorMgr) {
            editorMgr.ExitEditor();
        }
    }
    
    void OnFileSelectedMap(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.SetBaseMapModel(AssetManager.Instance.CreateNewAssetFromFile(files[0]));
            // Open Geo localization
            EditorManager.Instance.ChangeState(AppState.GeoLocalization);
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }
    

}
