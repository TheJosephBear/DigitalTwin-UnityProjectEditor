using Dummiesman;
using System;
using System.IO;
using UnityEngine;

public class ModelImporting : MonoBehaviour {

    [Header("Source Paths")]
    public string objFilePath; // full path to your .obj file
    public string mtlFilePath; // full path to your .mtl file
    public string textureFilePath; // full path to your texture .png

    private string persistentFolder;

    void Start() {
        persistentFolder = Path.Combine(Application.persistentDataPath, "RuntimeObjs");

        // Make sure folder exists
        if (!Directory.Exists(persistentFolder))
            Directory.CreateDirectory(persistentFolder);

        // Copy files to persistentDataPath
        string objDest = CopyFileToPersistent(objFilePath);
        string mtlDest = CopyFileToPersistent(mtlFilePath);
        string textureDest = CopyFileToPersistent(textureFilePath);

        // Load OBJ
        LoadObjWithTexture(objDest, textureDest);
    }

    string CopyFileToPersistent(string sourcePath) {
        if (!File.Exists(sourcePath)) {
            Debug.LogError("File not found: " + sourcePath);
            return null;
        }

        string fileName = Path.GetFileName(sourcePath);
        string destPath = Path.Combine(persistentFolder, fileName);
        File.Copy(sourcePath, destPath, true);
        return destPath;
    }

    void LoadObjAtRuntime(string objPath) {
        if (string.IsNullOrEmpty(objPath)) {
            Debug.LogError("OBJ path is null or empty.");
            return;
        }

        // Dummiesman requires the OBJ file path
        GameObject loadedObj = new OBJLoader().Load(objPath);

        if (loadedObj != null) {
            loadedObj.transform.position = Vector3.zero;
            loadedObj.transform.localScale = Vector3.one;

            Debug.Log("OBJ loaded successfully!");
        } else {
            Debug.LogError("Failed to load OBJ.");
        }

        PrintCopiedFiles(persistentFolder);

        if (loadedObj != null) {
            var renderers = loadedObj.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers) {
                Debug.Log("Renderer: " + rend.name + ", Material: " + rend.sharedMaterial.name + ", Has mainTexture: " + (rend.sharedMaterial.mainTexture != null));
            }
        }

    }

    public void LoadObjWithTexture(string objPath, string texturePath) {
        GameObject loadedObj = new OBJLoader().Load(objPath);

        if (loadedObj == null) {
            Debug.LogError("Failed to load OBJ.");
            return;
        }

        // Load texture manually
        if (!File.Exists(texturePath)) {
            Debug.LogError("Texture file not found: " + texturePath);
            return;
        }

        byte[] fileData = File.ReadAllBytes(texturePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // Load PNG/JPG into Texture2D

        // Apply texture to all materials
        Renderer[] renderers = loadedObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers) {
            foreach (var mat in rend.materials) {
                mat.mainTexture = texture;
            }
        }

        Debug.Log("OBJ loaded with texture applied!");
    }


    public void PrintCopiedFiles(string folderPath) {
        if (!Directory.Exists(folderPath)) {
            Debug.LogError("Folder does not exist: " + folderPath);
            return;
        }

        Debug.Log("=== Files in persistent folder ===");
        string[] files = Directory.GetFiles(folderPath);
        foreach (string file in files) {
            Debug.Log("Found file: " + file);
        }
        Debug.Log("===============================");
    }

}
