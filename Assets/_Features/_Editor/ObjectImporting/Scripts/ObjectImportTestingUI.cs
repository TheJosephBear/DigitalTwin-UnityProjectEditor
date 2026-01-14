using Dummiesman;
using System;
using System.IO;
using UnityEngine;

public class ObjectImportTestingUI : MonoBehaviour {

    public void OnUpload() {
        FileBrowserManager.Instance.ShowLoadDialog(NactiModelVole);
    }

    public void OnDownload() {

    }

    void NactiModelVole(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
            AssetManager.Instance.CreateNewAsset(files[0]);
   //     FileLoadingManager.Instance.UploadFromPC(files[0].fileInfo.path);

    }


}
