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
            WebCommunicationManager.Instance.UploadFileToServer(modelAsset.filePath, modelAsset.ModelID);
        }
    }

    public ModelAsset FindModelAssetByID(string id) {
        return null;
    }

    public string SerializeAssetList() {
        List<SerializableModelAsset> serializableAssets = new List<SerializableModelAsset>();

        foreach (ModelAsset asset in assets) {
            SerializableModelAsset serializableAsset = new SerializableModelAsset {
                modelID = asset.ModelID,
                fileHash = asset.FileHash
            };
            serializableAssets.Add(serializableAsset);
        }

        return JsonUtility.ToJson(serializableAssets);
    }

    public void DeserializeAssetList(string json) {
        List<SerializableModelAsset> serializableAssets = JsonUtility.FromJson<List<SerializableModelAsset>>(json);

        foreach (SerializableModelAsset serializableAsset in serializableAssets) {
            ModelAsset existingAsset = FindModelAssetByID(serializableAsset.modelID);
            if (existingAsset == null) {
                // I MUST FIGURE OUT HOW TO DOWNLOAD AND USE THE MODELS FIRST
                /*
                GameObject newAssetGo = FileLoading.Instance.LoadModel(serializableAsset.filePath);
                newAssetGo.transform.parent = AssetContainer.transform;
                ModelAsset newAsset = newAssetGo.AddComponent<ModelAsset>();
                newAsset.ModelID = serializableAsset.modelID;
                newAsset.FileHash = serializableAsset.fileHash;
                newAsset.SetModelGameObject(newAssetGo);
                assets.Add(newAsset);
                */
            }
        }
    }
}

