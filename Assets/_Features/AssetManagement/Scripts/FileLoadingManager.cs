using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using UnityEngine;
using System.Linq;

/// <summary>
/// Responsible for creating GameObjects from files from any source.
/// </summary>
/// <remarks>
/// For creating GameObjects that will be part of the Digital twin project, use <see cref="AssetManager"/> instead.
/// </remarks>
public class FileLoadingManager : Singleton<FileLoadingManager> {



    #region Public interface for uploading from pc

    public GameObject UploadFromPC(string folderOrObjPath, string fileHash) {
        ResolveOriginalPaths(folderOrObjPath, out string objPath, out string folderPath);

        string copiedObjPath = CopyObjBundleToPersistent(objPath, folderPath, fileHash);

        return BuildObjFromCopiedFiles(copiedObjPath);
    }

    #endregion

    #region Public interface for downloading from server

    /// <summary>
    /// Creates an OBJ file from downloaded byte data and stores it
    /// in the asset's persistent directory.
    /// </summary>
    public void CreateOBJFromBytes(string assetHash, string fileName, byte[] data) {
        WriteFile(assetHash, fileName, data);
    }

    /// <summary>
    /// Creates an MTL file from downloaded byte data and stores it
    /// in the asset's persistent directory.
    /// </summary>
    public void CreateMTLFromBytes(string assetHash, string fileName, byte[] data) {
        WriteFile(assetHash, fileName, data);
    }

    /// <summary>
    /// Creates a texture file (PNG/JPG) from downloaded byte data and stores it
    /// in the asset's persistent directory.
    /// Multiple textures can be added for the same asset.
    /// </summary>
    public void CreateTextureFromBytes(string assetHash, string fileName, byte[] data) {
        WriteFile(assetHash, fileName, data);
    }

    /// <summary>
    /// Builds a model from files that were previously created from byte data.
    /// All required files must already exist in the asset directory.
    /// </summary>
    /// <param name="assetHash">Unique hash identifying this model asset.</param>
    /// <returns>Instantiated model GameObject, deactivated by default.</returns>
    public GameObject BuildFromDownloadedFiles(string assetHash) {
        string root = GetPersistentAssetPath(assetHash);

        string[] objFiles = Directory.GetFiles(root, "*.obj");
        if (objFiles.Length == 0) {
            Debug.LogError($"No OBJ file found in asset folder: {root}");
            return null;
        }

        // Use the first OBJ file found
        return BuildObjFromCopiedFiles(objFiles[0]);
    }


    #endregion

    public string GetPathToFiles(string assetHash) {
        return GetPersistentAssetPath(assetHash);
    }

    /// <summary>
    /// Returns all files for a given asset identified by its fileHash.
    /// Includes OBJ, MTL, textures, or any other files stored in the asset folder.
    /// </summary>
    public List<string> GetAllFilesForAsset(string fileHash) {
        string folderPath = GetPathToFiles(fileHash);

        if (!Directory.Exists(folderPath))
            return new List<string>(); // empty list if folder doesn't exist

        return new List<string>(Directory.GetFiles(folderPath));
    }



    #region Object loading from PC helpers

    private void ResolveOriginalPaths(string folderOrObjPath, out string objPath, out string folderPath) {
        if (Path.GetExtension(folderOrObjPath).ToLower() == ".obj") {
            objPath = folderOrObjPath;
            folderPath = Path.GetDirectoryName(folderOrObjPath);
        } else {
            folderPath = folderOrObjPath;
            string[] objs = Directory.GetFiles(folderPath, "*.obj");

            if (objs.Length == 0)
                throw new FileNotFoundException("No OBJ file found.");

            objPath = objs[0];
        }
    }

    private string CopyObjBundleToPersistent(string objPath, string sourceFolder, string fileHash) {
        string targetRoot = GetPersistentAssetPath(fileHash);
        Directory.CreateDirectory(targetRoot);

        // Copy OBJ
        string targetObjPath = Path.Combine(targetRoot, Path.GetFileName(objPath));
        File.Copy(objPath, targetObjPath, true);

        // Copy MTL if exists
        string sourceMtlPath = Path.ChangeExtension(objPath, ".mtl");
        if (!File.Exists(sourceMtlPath))
            return targetObjPath;

        string targetMtlPath = Path.Combine(targetRoot, Path.GetFileName(sourceMtlPath));
        File.Copy(sourceMtlPath, targetMtlPath, true);

        // Copy textures referenced by MTL
        foreach (string tex in ExtractTexturesFromMtl(sourceMtlPath, sourceFolder)) {
            string targetTexPath = Path.Combine(targetRoot, Path.GetFileName(tex));
            File.Copy(tex, targetTexPath, true);
        }

        return targetObjPath;
    }

    private IEnumerable<string> ExtractTexturesFromMtl(string mtlPath, string folder) {
        foreach (string raw in File.ReadAllLines(mtlPath)) {
            string line = raw.Trim();
            if (!line.StartsWith("map_Kd "))
                continue;

            string texName = line.Substring(7).Trim();
            string fullPath = Path.Combine(folder, Path.GetFileName(texName));

            if (File.Exists(fullPath))
                yield return fullPath;
        }
    }

    #endregion

    #region Object building (for when you have all the files uploaded to unity)

    private GameObject BuildObjFromCopiedFiles(string copiedObjPath) {
        GameObject obj = new OBJLoader().Load(copiedObjPath);
        obj.SetActive(false);

        string copiedFolder = Path.GetDirectoryName(copiedObjPath);
        string copiedMtlPath = Path.ChangeExtension(copiedObjPath, ".mtl");

        if (File.Exists(copiedMtlPath))
            ApplyMaterialsFromMtl(obj, copiedMtlPath, copiedFolder);

        return obj;
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

    #endregion

    #region Download from server helpers

    /// <summary>
    /// Writes a file from raw byte data into the asset's persistent directory.
    /// </summary>
    private void WriteFile(string assetHash, string fileName, byte[] data) {
        string root = GetPersistentAssetPath(assetHash);
        string path = Path.Combine(root, fileName);
        File.WriteAllBytes(path, data);
    }

    #endregion

    /// <summary>
    /// Returns the persistent data folder path for a given asset hash.
    /// </summary>
    private string GetPersistentAssetPath(string fileHash) {
        string path = Path.Combine(Application.persistentDataPath, fileHash);
        Directory.CreateDirectory(path);
        return path;
    }
}

/* THE HOLY WORKING CODE */
/*

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
*/

/* OLD BYTE CREATION */
/*
    public GameObject LoadModel(byte[] file) {
        using (MemoryStream memoryStream = new MemoryStream(file)) {
            // Load the OBJ model from the MemoryStream
            GameObject loadedObject = new OBJLoader().Load(memoryStream);
            loadedObject.SetActive(false);
            return loadedObject;
        }
    }
*/