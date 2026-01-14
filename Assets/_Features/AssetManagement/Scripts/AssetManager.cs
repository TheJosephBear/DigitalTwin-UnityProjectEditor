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

    public void UploadModelsToWeb() {
        foreach (ModelAsset modelAsset in assets) {
            List<string> pathsToFiles = FileLoadingManager.Instance.GetAllFilesForAsset(modelAsset.FileHash);
            print(pathsToFiles.Count);
            foreach (string path in pathsToFiles) {
                print("saving path: "+path);
                string fileName = Path.GetFileName(path);
                ServerCommunicationManager.Instance.UploadFileToServer(
                    path,
                    fileName,
                    ProjectManager.Instance.SelectedProject.ProjectName,
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
   /*         bool isDone = false;
            DownloadAsset(
                objectID: serializableAsset.modelID,
                projectName: ProjectManager.Instance.SelectedProject.ProjectName, 
                onComplete: modelAsset => {
                if (modelAsset != null) {
                    modelAsset.AssetID = serializableAsset.modelID;
                    modelAsset.FileHash = serializableAsset.fileHash;
                }
                isDone = true;
            });
            yield return new WaitUntil(() => isDone);*/
        }
        onComplete?.Invoke();
        yield break;
    }

    /// <summary>
    /// Function that downloads the asset files based on the asset ID.
    /// Used in deseralization process
    /// </summary>
    /// <param name="objectID"></param>
    /// <param name="projectName"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    ModelAsset DownloadAsset(string objectID, string projectName, System.Action<ModelAsset> onComplete) {
        ModelAsset modelAsset = null;
        ServerCommunicationManager.Instance.DownloadFileFromServer(objectID, projectName, fileData => {
            if (fileData == null) {
                Debug.LogError("Failed to download model file.");
                onComplete(null);
                return;
            }

            string fileHash = GetFileHash(fileData);

            // Check if the model is already loaded based on its ID
            foreach (var asset in assets) {
                if (asset.FileHash == fileHash) {
                    Debug.Log("Model already uploaded.");
                    onComplete(asset);
                    return;
                }
            }

            // Load the model directly from the byte data without saving to file
            modelAsset = CreateNewAssetFromByteArray(fileData, fileHash, onComplete);
        });

        return modelAsset;
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