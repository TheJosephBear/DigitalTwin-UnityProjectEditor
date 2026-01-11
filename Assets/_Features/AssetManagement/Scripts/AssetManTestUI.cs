using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetManTestUI : MonoBehaviour {

    public string ObjectIDToDownload;
    public string ProjectName; //project from which you download the object

    public void OnUpload() {
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelected);
    }

    public void OnDownload() {
        AssetManager.Instance.DownloadModel(ObjectIDToDownload, ProjectName, modelAsset => {
            print(modelAsset.name);
            print(modelAsset.filePath);
            print(modelAsset.name);

            modelAsset.gameObject.SetActive(true);
        });
  
    }
    
    void OnFileSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            ModelAsset createdAsset = AssetManager.Instance.CreateNewAsset(files[0]);
            createdAsset.gameObject.SetActive(true);
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }
    }
    
}
