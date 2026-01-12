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

        string targetDir = Application.persistentDataPath + "/Capobara/";
        Directory.CreateDirectory(targetDir);

        File.Copy(
            @"C:\Users\josef\Desktop\Capobara\Capybara09180.obj",
            targetDir + "Capybara09180.obj",
            true
        );

        new OBJLoader().Load(targetDir + "Capybara09180.obj");

    }


}
