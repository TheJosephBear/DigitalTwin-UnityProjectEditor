using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using UnityEngine;

public class FileLoadingManager : Singleton<FileLoadingManager> { 
    
    public GameObject LoadModel(byte[] file) {
        using (MemoryStream memoryStream = new MemoryStream(file)) {
            // Load the OBJ model from the MemoryStream
            GameObject loadedObject = new OBJLoader().Load(memoryStream);
            loadedObject.SetActive(false);
            return loadedObject;
        }
    }

    /// <summary>
    /// Loads a model from a folder containing OBJ + MTL + texture files.
    /// Automatically parses the MTL and applies textures to the materials.
    /// If there is no MTL or textures, the model will be created without them.
    /// </summary>
    /// <param name="folderPath">Folder containing OBJ, MTL, and texture files.</param>
    /// <param name="objFileName">Name of the OBJ file inside the folder.</param>
    /// <returns>Loaded GameObject with materials applied, deactivated by default.</returns>
    public GameObject LoadObj(string folderOrObjPath) {
        // Determine if the input contains a .obj file
        string folderPath;
        string objFileName;

        if (Path.GetExtension(folderOrObjPath).ToLower() == ".obj") {
            // Full OBJ path provided extract folder and filename
            folderPath = Path.GetDirectoryName(folderOrObjPath);
            objFileName = Path.GetFileName(folderOrObjPath);
        } else {
            // Folder path provided assume the folder contains exactly one OBJ
            folderPath = folderOrObjPath;

            string[] objFiles = Directory.GetFiles(folderPath, "*.obj");
            if (objFiles.Length == 0) {
                Debug.LogError($"No OBJ file found in folder: {folderPath}");
                return null;
            }

            objFileName = Path.GetFileName(objFiles[0]); // take the first OBJ
        }

        string objPath = Path.Combine(folderPath, objFileName);
        string mtlPath = Path.ChangeExtension(objPath, ".mtl");

        if (!File.Exists(objPath)) {
            Debug.LogError($"OBJ file not found: {objPath}");
            return null;
        }

        GameObject loadedObj = new OBJLoader().Load(objPath);
        loadedObj.SetActive(false);

        if (!File.Exists(mtlPath)) {
            Debug.LogWarning($"No MTL file found. OBJ loaded without textures.");
            return loadedObj;
        }

        ApplyMaterialsFromMtl(loadedObj, mtlPath, folderPath);

        return loadedObj;
    }


    /// <summary>
    /// Parses an MTL file and applies the textures to the materials on the loaded model.
    /// </summary>
    private void ApplyMaterialsFromMtl(GameObject obj, string mtlPath, string folder) {
        string[] lines = File.ReadAllLines(mtlPath);
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

                Debug.Log($"Applied texture {texPath} -> material {cleanName}");
            }
        }
    }

}
