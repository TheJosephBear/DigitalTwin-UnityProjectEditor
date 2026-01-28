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
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            EditorManager.Instance.MapManager.SetBaseMapModel(createdAsset);
        });
    }

    public void onPridatVariantu() {
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            EditorManager.Instance.MapManager.UploadMapVariant(createdAsset);
        });
    }

    
}