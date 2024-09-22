using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking.Types;

public class AssetManager : Singleton<AssetManager> {
    /// <summary>
    /// Creates assets from uploaded models for easier upload and download
    /// </summary>
    
    public GameObject AssetContainer; // parent gameobject to all uploaded models that will turn to assets
    List<ModelAsset> assets = new List<ModelAsset>();

    public ModelAsset CreateNewAsset(string path) {
        // Duplication check
        string fileHash = GetFileHash(path);
        foreach (var asset in assets) {
            if (asset.FileHash == fileHash) {
                Debug.Log("Model already uploaded.");
                return asset;
            }
        }
        // New asset creation
        GameObject newAssetGo = FileLoading.Instance.LoadModel(path);
        newAssetGo.transform.parent = AssetContainer.transform;
        ModelAsset modelAsset = newAssetGo.AddComponent<ModelAsset>();
        modelAsset.GenerateUniqueID();
        modelAsset.FileHash = fileHash;
        modelAsset.filePath = path;
        modelAsset.SetModelGameObject(newAssetGo);
        assets.Add(modelAsset);
        newAssetGo.SetActive(false);
        return modelAsset;
    }

    string GetFileHash(string filePath) {
        using (var md5 = MD5.Create()) {
            using (var stream = File.OpenRead(filePath)) {
                byte[] hashBytes = md5.ComputeHash(stream);
                return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public void UploadModelsToWeb() {
        foreach(ModelAsset modelAsset in assets) {
            WebCommunicationManager.Instance.UploadFileToServer(modelAsset.filePath, modelAsset.ModelID, ProjectSaver.Instance.project.ProjectName);
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

    void DownloadModel(string objectID, System.Action<ModelAsset> onComplete) {
        WebCommunicationManager.Instance.DownloadFileFromServer(objectID, ProjectSaver.Instance.project.ProjectName, fileData => {
            if (fileData == null) {
                Debug.LogError("Failed to download model file.");
                onComplete(null);
                return;
            }

            string localPath = Path.Combine(Application.persistentDataPath, objectID + ".obj");
            File.WriteAllBytes(localPath, fileData);

            LoadModelAsset(localPath, onComplete);
        });
    }

    void LoadModelAsset(string path, System.Action<ModelAsset> onComplete) {
        string fileHash = GetFileHash(path);

        foreach (var asset in assets) {
            if (asset.FileHash == fileHash) {
                Debug.Log("Model already uploaded.");
                onComplete(asset);
                return;
            }
        }
        GameObject newAssetGo = FileLoading.Instance.LoadModel(path);
        print("model is loaded in the assetManager");
        newAssetGo.transform.parent = AssetContainer.transform;

        ModelAsset modelAsset = newAssetGo.AddComponent<ModelAsset>();
        modelAsset.filePath = path;
        modelAsset.SetModelGameObject(newAssetGo);
        assets.Add(modelAsset);
        newAssetGo.SetActive(false);
        onComplete(modelAsset);
    }

    public void DeserializeAssetList(List<SerializableModelAsset> data, System.Action onComplete = null) {
        StartCoroutine(DeserializeAssetsCoroutine(data, onComplete));
    }

    IEnumerator DeserializeAssetsCoroutine(List<SerializableModelAsset> data, System.Action onComplete) {
        foreach (SerializableModelAsset serializableAsset in data) {
            bool isDone = false;
            DownloadModel(serializableAsset.modelID, modelAsset => {
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