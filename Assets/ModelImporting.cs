using Dummiesman;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ModelImporting : MonoBehaviour {
    [Header("Source Folder (contains .obj, .mtl, textures)")]
    public string modelFolderPath; // e.g., "C:/Users/User/Desktop/CityBuilding"

    private string persistentFolder;

    void Start() {
        ClearPersistentRuntimeFolder();
            
        if (string.IsNullOrEmpty(modelFolderPath) || !Directory.Exists(modelFolderPath)) {
            Debug.LogError("Model folder path is invalid: " + modelFolderPath);
            return;
        }

        persistentFolder = Path.Combine(Application.persistentDataPath, "RuntimeObjs");
        Directory.CreateDirectory(persistentFolder);

        CopyFolderToPersistent(modelFolderPath, persistentFolder);

        string objPath = FindFileByExtension(persistentFolder, ".obj");
        if (objPath == null) {
            Debug.LogError("No OBJ file found in folder!");
            return;
        }

        LoadObjWithMaterials(objPath);
    }

    #region File Copy Utilities
    private void CopyFolderToPersistent(string sourceFolder, string targetFolder) {
        foreach (string filePath in Directory.GetFiles(sourceFolder)) {
            string destPath = Path.Combine(targetFolder, Path.GetFileName(filePath));
            File.Copy(filePath, destPath, true);
            Debug.Log("Copied file: " + destPath);
        }
    }

    private string FindFileByExtension(string folder, string extension) {
        string[] files = Directory.GetFiles(folder, "*" + extension);
        return files.Length > 0 ? files[0] : null;
    }
    #endregion

    #region Runtime OBJ & Material Loader
    private void LoadObjWithMaterials(string objPath) {
        GameObject loadedObj = new OBJLoader().Load(objPath);
        if (loadedObj == null) {
            Debug.LogError("Failed to load OBJ: " + objPath);
            return;
        }

        string folder = Path.GetDirectoryName(objPath);
        string mtlPath = Path.ChangeExtension(objPath, ".mtl");

        if (File.Exists(mtlPath)) {
            ApplyMaterialsFromMtl(loadedObj, mtlPath, folder);
        } else {
            Debug.LogWarning("No MTL file found. OBJ loaded without materials.");
        }

        loadedObj.transform.position = Vector3.zero;
        loadedObj.transform.localScale = Vector3.one;

        Debug.Log("OBJ loaded successfully: " + loadedObj.name);
    }

    private void ApplyMaterialsFromMtl(GameObject obj, string mtlPath, string folder) {
        string[] lines = File.ReadAllLines(mtlPath);

        // Map: material name  texture path
        Dictionary<string, string> materialToTexture = new Dictionary<string, string>();
        string currentMaterial = null;

        foreach (string rawLine in lines) {
            string line = rawLine.Trim();

            if (line.StartsWith("newmtl ")) {
                currentMaterial = line.Substring(7).Trim();
            } else if (line.StartsWith("map_Kd ") && currentMaterial != null) {
                string texName = line.Substring(7).Trim();
                string texPath = Path.Combine(folder, Path.GetFileName(texName));
                materialToTexture[currentMaterial] = texPath;
            }
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            foreach (Material mat in renderer.materials) {
                string cleanName = mat.name.Replace(" (Instance)", "");

                if (!materialToTexture.TryGetValue(cleanName, out string texPath))
                    continue;

                if (!File.Exists(texPath)) {
                    Debug.LogWarning($"Texture not found for {cleanName}: {texPath}");
                    continue;
                }

                byte[] data = File.ReadAllBytes(texPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(data);

                mat.mainTexture = tex;
                Debug.Log($"Applied texture {texPath} material {cleanName}");
            }
        }
    }

    #endregion


    private void ClearPersistentRuntimeFolder() {
        string runtimeFolder = Path.Combine(Application.persistentDataPath, "RuntimeObjs");

        if (!Directory.Exists(runtimeFolder))
            return;

        try {
            DirectoryInfo dir = new DirectoryInfo(runtimeFolder);

            foreach (FileInfo file in dir.GetFiles()) {
                file.Delete();
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories()) {
                subDir.Delete(true);
            }

            Debug.Log("Persistent runtime folder cleared: " + runtimeFolder);
        } catch (Exception e) {
            Debug.LogError("Failed to clear persistent runtime folder: " + e.Message);
        }
    }

}
