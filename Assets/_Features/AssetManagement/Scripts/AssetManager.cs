using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class AssetManager : Singleton<AssetManager> {
    /// <summary>
    /// Creates assets from uploaded models for easier upload and download
    /// </summary>
    
    public GameObject AssetContainer; // Parent gameobject for uploaded models
    List<ModelAsset> assets = new List<ModelAsset>();
    
    public ModelAsset CreateNewAssetFromFile(FrostweepGames.Plugins.WebGLFileBrowser.File file) {
        // Duplication check
        string fileHash = GetFileHash(file.data);
        foreach (var asset in assets) {
            if (asset.FileHash == fileHash) {
                Debug.Log("Model already uploaded.");
                return asset;
            }
        }

        // New asset creation
        GameObject newAssetGo = FileLoadingManager.Instance.UploadFromPC(file.fileInfo.path, fileHash);
        newAssetGo.transform.parent = AssetContainer.transform;
        ModelAsset modelAsset = newAssetGo.AddComponent<ModelAsset>();
        modelAsset.FileName = file.fileInfo.fullName;
        modelAsset.FileHash = fileHash;
        modelAsset.SetModelGameObject(newAssetGo);
        assets.Add(modelAsset);
        newAssetGo.SetActive(false);
        return modelAsset;
    }

    public ModelAsset FindModelAssetByFileHash(string fileHash) {
        foreach (ModelAsset modelAsset in assets) {
            if (modelAsset.FileHash == fileHash) return modelAsset;
        }
        print("modelAsset with fileHash " + fileHash + " doesn't exist.");
        return null;
    }

    public void ClearManager() {
        foreach (ModelAsset modelAsset in assets) {
            Destroy(modelAsset.gameObject);
        }
        assets.Clear();
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

        foreach (ModelAsset asset in assets) {
            SerializableModelAsset serializableAsset = new SerializableModelAsset {
                fileHash = asset.FileHash
            };
            serializableAssets.Add(serializableAsset);
        }

        return serializableAssets;
    }

    public void UploadModelsToWeb(string projectNameToUploadTo) {
        foreach (ModelAsset modelAsset in assets) {
            List<string> pathsToFiles = FileLoadingManager.Instance.GetAllFilesForAsset(modelAsset.FileHash);
            print(pathsToFiles.Count);
            foreach (string path in pathsToFiles) {
                print("saving path: "+path);
                string fileName = Path.GetFileName(path);
                ServerCommunicationManager.Instance.UploadFileToServer(
                    path,
                    fileName,
                    projectNameToUploadTo,
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
        foreach (var asset in assets) {
            if (asset.FileHash == assetHash) {
                onComplete(asset);
                return asset;
            }
        }

        StartCoroutine(DownloadAssetCoroutine(assetHash, projectName, onComplete));
        return null;
    }

    IEnumerator DownloadAssetCoroutine(string assetHash, string projectName, System.Action<ModelAsset> onComplete) {

        // get file list
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
            onComplete(null);
            yield break;
        }

        // download each file
        foreach (string fileName in fileNames) {
            bool fileDone = false;

            ServerCommunicationManager.Instance.DownloadFileFromServer(
                projectName,
                assetHash,
                fileName,
                data => {
                    if (data == null) {
                        fileDone = true;
                        return;
                    }

                    // decide type by extension
                    string ext = Path.GetExtension(fileName).ToLower();

                    if (ext == ".obj")
                        FileLoadingManager.Instance.CreateOBJFromBytes(assetHash, fileName, data);
                    else if (ext == ".mtl")
                        FileLoadingManager.Instance.CreateMTLFromBytes(assetHash, fileName, data);
                    else
                        FileLoadingManager.Instance.CreateTextureFromBytes(assetHash, fileName, data);

                    fileDone = true;
                });

            yield return new WaitUntil(() => fileDone);
        }

        // build GameObject
        GameObject go = FileLoadingManager.Instance.BuildFromDownloadedFiles(assetHash);
        go.transform.parent = AssetContainer.transform;
        go.SetActive(false);

        ModelAsset modelAsset = go.AddComponent<ModelAsset>();
        modelAsset.FileHash = assetHash;
        modelAsset.SetModelGameObject(go);

        assets.Add(modelAsset);
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
        assets.Add(modelAsset);
        newAssetGo.SetActive(false);
        onComplete(modelAsset);
        return modelAsset;
    }

    #endregion

}