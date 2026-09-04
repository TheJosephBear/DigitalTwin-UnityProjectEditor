using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class AssetManager : Singleton<AssetManager> {
    /// <summary>
    /// Creates _assets from uploaded models for easier upload and download
    /// </summary>

    public GameObject AssetContainer; // Parent gameobject for uploaded models
    List<ModelAsset> _assets = new List<ModelAsset>();

    public ModelAsset CreateNewAssetFromFile(FrostweepGames.Plugins.WebGLFileBrowser.File file) {
        // Duplication check
        string fileHash = GetFileHash(file.data);
        foreach (var asset in _assets) {
            if (asset.FileHash == fileHash) {
                Debug.Log("Model already uploaded.");
                return asset;
            }
        }

        // New asset creation
        GameObject newAssetGo = FileLoadingManager.Instance.UploadFromWebGLFile(file, fileHash);
        newAssetGo.transform.parent = AssetContainer.transform;
        ModelAsset modelAsset = newAssetGo.AddComponent<ModelAsset>();
        modelAsset.FileName = file.fileInfo.fullName;
        modelAsset.FileHash = fileHash;
        modelAsset.SetModelGameObject(newAssetGo);
        _assets.Add(modelAsset);
        newAssetGo.SetActive(false);

        // Save project state after model upload
        if(MainManagerBase.Instance is EditorManager editorMan) {
            MessageDisplayManager.Instance.DisplayMessage("Aseet Manager save");
            editorMan.SaveProject();
        }

        return modelAsset;
    }

    public ModelAsset CreateNewAssetFromFiles(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files == null || files.Length == 0) {
            Debug.LogError("No files selected.");
            return null;
        }

        // 1. Find OBJ file NOT ANYMOREEEEE
        // We are looking for any main file
        FrostweepGames.Plugins.WebGLFileBrowser.File mainFile = null;
        foreach (var f in files) {
            if (FileLoadingManager.Instance.IsMainModelExtension(f.fileInfo.extension.ToLower())) {
                mainFile = f;
                break;
            }
        }

        if (mainFile == null) {
            Debug.LogError("No main file in selection.");
            return null;
        }

        // 2. Hash based on main file
        string fileHash = GetFileHash(mainFile.data);

        // 3. Duplication check
        foreach (var asset in _assets) {
            if (asset.FileHash == fileHash) {
                Debug.Log("Model already uploaded.");
                return asset;
            }
        }

        // 4. Upload full bundle
        GameObject newAssetGo =
            FileLoadingManager.Instance.UploadFromWebGLFiles(files, fileHash);

        if (newAssetGo == null)
            return null;

        // 5. Register asset
        newAssetGo.transform.parent = AssetContainer.transform;

        ModelAsset modelAsset = newAssetGo.AddComponent<ModelAsset>();
        modelAsset.FileName = mainFile.fileInfo.fullName;
        modelAsset.FileHash = fileHash;
        modelAsset.SetModelGameObject(newAssetGo);

        _assets.Add(modelAsset);
        newAssetGo.SetActive(false);

        print("Created asset with: "+ modelAsset.FileName);
        print("Created asset with: "+ modelAsset.FileHash);

        return modelAsset;
    }

    public ModelAsset FindModelAssetByFileHash(string fileHash) {
        foreach (ModelAsset modelAsset in _assets) {
            if (modelAsset.FileHash == fileHash) return modelAsset;
        }
        print("modelAsset with fileHash " + fileHash + " doesn't exist.");
        return null;
    }

    public void DestroyAsset(ModelAsset modelAsset) {
        print("Trying to destroy model with hash: " + modelAsset.FileHash);
        ModelAsset assetToRemove = FindModelAssetByFileHash(modelAsset.FileHash);
        _assets.Remove(assetToRemove);
        Destroy(assetToRemove.gameObject);
        Destroy(modelAsset.gameObject);
    }

    public void ClearManager() {
        foreach (ModelAsset modelAsset in _assets) {
            Destroy(modelAsset.gameObject);
        }
        _assets.Clear();
    }

    #region Helper functions

    string GenerateUniqueID() {
        return Guid.NewGuid().ToString();
    }

    string GetFileHash(byte[] fileData) {
        using (var md5 = MD5.Create()) {
            byte[] hashBytes = md5.ComputeHash(fileData);
            return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    #endregion

    #region Serialization

    public List<SerializableModelAsset> SerializeAssetList() {
        List<SerializableModelAsset> serializableAssets = new List<SerializableModelAsset>();

        foreach (ModelAsset asset in _assets) {
            SerializableModelAsset serializableAsset = new SerializableModelAsset {
                fileHash = asset.FileHash
            };
            serializableAssets.Add(serializableAsset);
        }

        return serializableAssets;
    }

    public void UploadModelsToWeb(string projectName) {
        foreach (ModelAsset modelAsset in _assets) {
            List<string> pathsToFiles = FileLoadingManager.Instance.GetAllFilesForAsset(modelAsset.FileHash);
            print(pathsToFiles.Count);
            foreach (string path in pathsToFiles) {
                string fileName = Path.GetFileName(path);
                ServerCommunicationManager.Instance.UploadFileToServer(
                    path,
                    fileName,
                    projectName,
                    modelAsset.FileHash
                );
            }
        }
    }


    #endregion

    #region Deseralization

    public void DeserializeAssetList(List<SerializableModelAsset> data, System.Action onComplete = null) {
        StartCoroutine(DeserializeAssetCoroutine(data, onComplete));
    }

    IEnumerator DeserializeAssetCoroutine(List<SerializableModelAsset> data, System.Action onComplete) {
        foreach (SerializableModelAsset serializableAsset in data) {
            bool isDone = false;

            DownloadAsset(
                assetHash: serializableAsset.fileHash,
                projectName: ProjectManager.Instance.SelectedProject.ProjectName,
                onComplete: modelAsset => {
                    if (modelAsset != null) {
                        modelAsset.FileHash = serializableAsset.fileHash;
                    }
                    isDone = true;
                });

            yield return new WaitUntil(() => isDone);
        }

        onComplete?.Invoke();
    }


    ModelAsset DownloadAsset(string assetHash, string projectName, System.Action<ModelAsset> onComplete) {
        // already loaded?
        foreach (var asset in _assets) {
            if (asset.FileHash == assetHash) {
                onComplete(asset);
                return asset;
            }
        }

        StartCoroutine(DownloadAssetCoroutine(assetHash, projectName, onComplete));
        return null;
    }

    IEnumerator DownloadAssetCoroutine(string assetHash, string projectName, System.Action<ModelAsset> onComplete) {
        // 1. Get the list of files associated with this asset hash
        List<string> fileNames = null;
        bool listDone = false;

        ServerCommunicationManager.Instance.ListFilesForAsset(
            projectName,
            assetHash,
            files => {
                fileNames = files;
                listDone = true;
            });

        yield return new WaitUntil(() => listDone);

        if (fileNames == null || fileNames.Count == 0) {
            Debug.LogError($"No files found on server for asset hash: {assetHash}");
            onComplete(null);
            yield break;
        }

        // 2. Track remaining file downloads in a counter
        int remainingDownloads = fileNames.Count;
        bool downloadFailed = false;

        foreach (string fileName in fileNames) {
            ServerCommunicationManager.Instance.DownloadFileFromServer(
                projectName,
                assetHash,
                fileName,
                data => {
                    if (data == null) {
                        Debug.LogError($"Failed to download file payload: {fileName}");
                        downloadFailed = true;
                        remainingDownloads--;
                        return;
                    }

                    // Process and cache the raw bytes based on file type
                    string ext = Path.GetExtension(fileName).ToLower();

                    if (ext == ".obj")
                        FileLoadingManager.Instance.CreateOBJFromBytes(assetHash, fileName, data);
                    else if (ext == ".mtl")
                        FileLoadingManager.Instance.CreateMTLFromBytes(assetHash, fileName, data);
                    else
                        FileLoadingManager.Instance.CreateTextureFromBytes(assetHash, fileName, data);

                    // Decrement counter once caching is secure
                    remainingDownloads--;
                });
        }

        // 3. Wait cleanly until ALL files are written safely to memory
        yield return new WaitUntil(() => remainingDownloads == 0);

        if (downloadFailed) {
            Debug.LogError($"Asset bundle construction aborted due to download failures: {assetHash}");
            onComplete(null);
            yield break;
        }

        // 4. Safe Zone: Construct the GameObject now that all matching files exist locally
        GameObject go = FileLoadingManager.Instance.BuildFromDownloadedFiles(assetHash);
        if (go == null) {
            Debug.LogError($"FileLoadingManager failed to construct model geometry for hash: {assetHash}");
            onComplete(null);
            yield break;
        }

        go.transform.parent = AssetContainer.transform;
        go.SetActive(false);

        ModelAsset modelAsset = go.AddComponent<ModelAsset>();
        modelAsset.FileHash = assetHash;
        // Set filename to the OBJ name so your UI layers display it accurately
        string objFileName = fileNames.Find(f => f.ToLower().EndsWith(".obj")) ?? "Unknown.obj";
        modelAsset.FileName = objFileName;
        modelAsset.SetModelGameObject(go);

        _assets.Add(modelAsset);
        onComplete(modelAsset);
    }


    /// <summary>
    /// Creating the asset gameobject with all data loaded in attached ModelAsset script.
    /// Used in deseralization process.
    /// </summary>
    /// <param name="fileData"></param>
    /// <param name="fileHash"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    ModelAsset CreateNewAssetFromByteArray(byte[] fileData, string fileHash, System.Action<ModelAsset> onComplete) {
        // Use FileLoadingManager to load the model from the byte array
        //     GameObject newAssetGo = FileLoadingManager.Instance.LoadModel(fileData);
        GameObject newAssetGo = null;

        if (newAssetGo == null) {
            Debug.LogError("Failed to load model from data.");
            onComplete(null);
            return null;
        }

        newAssetGo.transform.parent = AssetContainer.transform;

        // Set up the ModelAsset component with necessary properties
        ModelAsset modelAsset = newAssetGo.AddComponent<ModelAsset>();
        modelAsset.FileHash = fileHash;
        modelAsset.SetModelGameObject(newAssetGo);
        _assets.Add(modelAsset);
        newAssetGo.SetActive(false);
        onComplete(modelAsset);
        return modelAsset;
    }

    #endregion

}
