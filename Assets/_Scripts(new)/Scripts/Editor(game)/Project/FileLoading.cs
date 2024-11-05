using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using UnityEngine;

public class FileLoading : Singleton<FileLoading> { 

    protected override void Awake() {
        base.Awake();
    }

    public GameObject LoadModel(string path) {
        string extension = Path.GetExtension(path).ToLower();
        switch (extension) {
            case ".obj":
                GameObject loadingObject = new OBJLoader().Load(path);
                loadingObject.SetActive(false);
                return loadingObject;
            case ".fbx":
                PopUp.Instance.ShowPopUpWindow("We can't support FBX yet.");
                break;
            default:
                PopUp.Instance.ShowPopUpWindow("Selected file is not an OBJ model.");
                break;
        }
        return null;
    }

    public GameObject LoadModel(FrostweepGames.Plugins.WebGLFileBrowser.File file) {
        using (MemoryStream memoryStream = new MemoryStream(file.data)) {
            // Load the OBJ model from the MemoryStream
            GameObject loadedObject = new OBJLoader().Load(memoryStream);
            loadedObject.SetActive(false);
            return loadedObject;
        }
    }

    public GameObject LoadModel(byte[] file) {
        using (MemoryStream memoryStream = new MemoryStream(file)) {
            // Load the OBJ model from the MemoryStream
            GameObject loadedObject = new OBJLoader().Load(memoryStream);
            loadedObject.SetActive(false);
            return loadedObject;
        }
    }


}
