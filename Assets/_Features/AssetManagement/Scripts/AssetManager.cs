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
    
    public GameObject AssetContainer; // dropdownMultiview gameobject to all uploaded models that will turn to assets
    List<ModelAsset> assets = new List<ModelAsset>();

    public ModelAsset CreateNewAsset(FrostweepGames.Plugins.WebGLFileBrowser.File file) {
        // Duplication check
        print("trying to access path for hash");
        string fileHash = GetFileHash(file.data);
        foreach (var asset in assets) {
            if (asset.FileHash == fileHash) {
                Debug.Log("Model already uploaded.");
                return asset;
            }
        }
        // New asset creation
        print("calling the fileloading for loading the model via FILE");
        GameObject newAssetGo = FileLoading.Instance.LoadModel(file);
        newAssetGo.transform.parent = AssetContainer.transform;
        ModelAsset modelAsset = newAssetGo.AddComponent<ModelAsset>();
        modelAsset.FileName = file.fileInfo.fullName;
        modelAsset.GenerateUniqueID();
        modelAsset.FileHash = fileHash;
        modelAsset.filePath = file.fileInfo.path;
        modelAsset.SetModelGameObject(newAssetGo);
        assets.Add(modelAsset);
        newAssetGo.SetActive(false);
        return modelAsset;
    }

    public void ClearEverything() {
        foreach (ModelAsset modelAsset in assets) {
            Destroy(modelAsset.gameObject);
        }
        assets.Clear();
    }

    string GetFileHash(byte[] fileData) {
        using (var md5 = MD5.Create()) {
            byte[] hashBytes = md5.ComputeHash(fileData);
            return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public void UploadModelsToWeb() {
        foreach(ModelAsset modelAsset in assets) {
            ServerCommunicationManager.Instance.UploadFileToServer(modelAsset.filePath, modelAsset.ModelID, ProjectManager.Instance.SelectedProject.ProjectName);
        }
    }

    public List<SerializableModelAsset> SerializeAssetList() {
        List<SerializableModelAsset> serializableAssets = new List<SerializableModelAsset>();

        foreach (ModelAsset asset in assets) {
            SerializableModelAsset serializableAsset = new SerializableModelAsset {
                modelID = asset.ModelID,
                fileHash = asset.FileHash
            };
            serializableAssets.Add(serializableAsset);
        }

        foreach (SerializableModelAsset asset in serializableAssets) {
            print(asset.modelID);
            print(asset.fileHash);
        }

        return serializableAssets;
    }

    public ModelAsset FindModelAssetByID(string id) {
        foreach (ModelAsset modelAsset in assets) {
            if (modelAsset.ModelID == id) return modelAsset;
        }
        print("modelAsset with id " + id + " doesn't exist.");
        return null;
    }

    public ModelAsset DownloadModel(string objectID, string projectName, System.Action<ModelAsset> onComplete) {
        ModelAsset modelAsset = null;
        ServerCommunicationManager.Instance.DownloadFileFromServer(objectID, projectName, fileData => {
            if (fileData == null) {
                Debug.LogError("Failed to download model file.");
                onComplete(null);
                return;
            }

            string fileHash = GetFileHash(fileData);

            // Check if the model is already loaded based on its hash
            foreach (var asset in assets) {
                if (asset.FileHash == fileHash) {
                    Debug.Log("Model already uploaded.");
                    onComplete(asset);
                    return;
                }
            }

            // Load the model directly from the byte data without saving to file
            modelAsset = LoadModelAsset(fileData, fileHash, onComplete);
        });

        return modelAsset;
    }

    ModelAsset LoadModelAsset(byte[] fileData, string fileHash, System.Action<ModelAsset> onComplete) {
        // Use FileLoading to load the model from the byte array
        GameObject newAssetGo = FileLoading.Instance.LoadModel(fileData);

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

    public void DeserializeAssetList(List<SerializableModelAsset> data, System.Action onComplete = null) {
        StartCoroutine(DeserializeAssetsCoroutine(data, onComplete));
    }

    IEnumerator DeserializeAssetsCoroutine(List<SerializableModelAsset> data, System.Action onComplete) {
        foreach (SerializableModelAsset serializableAsset in data) {
            bool isDone = false;
            DownloadModel(
                objectID: serializableAsset.modelID,
                projectName: ProjectManager.Instance.SelectedProject.ProjectName, 
                onComplete: modelAsset => {
                if (modelAsset != null) {
                    modelAsset.ModelID = serializableAsset.modelID;
                    modelAsset.FileHash = serializableAsset.fileHash;
                }
                isDone = true;
            });
            yield return new WaitUntil(() => isDone);
        }
        onComplete?.Invoke();
    }
}