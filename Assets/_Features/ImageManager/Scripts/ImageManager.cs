using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;
using System.Collections;

public class ImageManager : Singleton<ImageManager> {

    public GameObject ImageContainer; // Parent for TextureAsset components
    private List<TextureAsset> _loadedTextures = new List<TextureAsset>();
    private Action<TextureAsset> _onTextureAssetLoadedCallback;

    #region Dialog Logic

    public void AskForImageDialog(Action<TextureAsset> callback) {
        _onTextureAssetLoadedCallback = callback;

        FileBrowserManager.Instance.ShowLoadDialog(OnImageFileSelected,
            filterExtensions: "png, jpg, jpeg",
            multipleSelection: false);
    }

    private void OnImageFileSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            TextureAsset asset = CreateTextureAsset(files[0]);
            _onTextureAssetLoadedCallback?.Invoke(asset);
        } else {
            if (MessageDisplayManager.Instance != null)
                MessageDisplayManager.Instance.ShowMessage("Prosím vyberte soubor typu .png/.jpg");
            _onTextureAssetLoadedCallback?.Invoke(null);
        }
        _onTextureAssetLoadedCallback = null;
    }

    #endregion

    #region Asset Creation

    public TextureAsset CreateTextureAsset(FrostweepGames.Plugins.WebGLFileBrowser.File file) {
        if (file == null || file.data == null) return null;

        // 1. Hash Check for Duplication
        string fileHash = GetFileHash(file.data);
        foreach (var existing in _loadedTextures) {
            if (existing.FileHash == fileHash) {
                Debug.Log("Texture already exists.");
                return existing;
            }
        }

        // 2. Create Texture2D
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(file.data)) return null;

        // 3. Create Wrapper Object
        GameObject go = new GameObject("Tex_" + file.fileInfo.name);
        if (ImageContainer != null) go.transform.parent = ImageContainer.transform;

        TextureAsset asset = go.AddComponent<TextureAsset>();
        asset.FileName = file.fileInfo.name;
        asset.FileHash = fileHash;
        asset.Texture = tex;

        _loadedTextures.Add(asset);

        // 4. Save State (similar to EditorManager logic)
        if (MainManagerBase.Instance is EditorManager editorMan) {
            editorMan.SaveProject();
        }

        return asset;
    }

    #endregion

    #region Serialization & Deserialization

    public serializableImageManager SerializeTextureList() {
        List<serializableTextureAsset> list = new List<serializableTextureAsset>();
        foreach (var asset in _loadedTextures) {
            list.Add(new serializableTextureAsset {
                fileName = asset.FileName,
                fileHash = asset.FileHash
            });
        }

        return new serializableImageManager {
            SerializedTextureList = list
        };
    }

    public void Deserialize(serializableImageManager data, Action onComplete = null) {
        StartCoroutine(DeserializeTexturesCoroutine(data.SerializedTextureList, onComplete));
    }

    private IEnumerator DeserializeTexturesCoroutine(List<serializableTextureAsset> data, Action onComplete) {
        foreach (var serializableAsset in data) {
            bool isDone = false;

            DownloadTextureAsset(
                serializableAsset.fileHash,
                serializableAsset.fileName,
                ProjectManager.Instance.SelectedProject.ProjectName,
                success => {
                    isDone = true;
                });

            yield return new WaitUntil(() => isDone);
        }

        onComplete?.Invoke();
    }

    private void DownloadTextureAsset(string hash, string fileName, string projectName, Action<bool> onComplete) {
        // Kontrola, zda už není v pamìti
        foreach (var asset in _loadedTextures) {
            if (asset.FileHash == hash) {
                onComplete?.Invoke(true);
                return;
            }
        }

        ServerCommunicationManager.Instance.DownloadFileFromServer(
            projectName,
            hash,
            fileName,
            data => {
                if (data != null) {
                    CreateTextureAssetFromBytes(data, hash, fileName);
                    onComplete?.Invoke(true);
                } else {
                    Debug.LogError($"Failed to download texture: {fileName}");
                    onComplete?.Invoke(false);
                }
            });
    }

    /// <summary>
    /// Pomocná metoda pro vytvoøení assetu z bytù (použito pøi deserializaci)
    /// </summary>
    private void CreateTextureAssetFromBytes(byte[] data, string hash, string fileName) {
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(data)) {
            GameObject go = new GameObject("Tex_" + fileName);
            if (ImageContainer != null) go.transform.parent = ImageContainer.transform;

            TextureAsset asset = go.AddComponent<TextureAsset>();
            asset.FileName = fileName;
            asset.FileHash = hash;
            asset.Texture = tex;

            _loadedTextures.Add(asset);
        }
    }

    #endregion

    public void ClearManager() {
        foreach (var asset in _loadedTextures) {
            if (asset.Texture != null) Destroy(asset.Texture);
            Destroy(asset.gameObject);
        }
        _loadedTextures.Clear();
    }

    #region Helpers

    private string GetFileHash(byte[] fileData) {
        using (var md5 = MD5.Create()) {
            byte[] hashBytes = md5.ComputeHash(fileData);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    #endregion
}

[System.Serializable]
public class serializableImageManager : MonoBehaviour {
    public List<serializableTextureAsset> SerializedTextureList;
}

[System.Serializable]
public class TextureAsset : MonoBehaviour {
    public string FileName;
    public string FileHash;
    public Texture2D Texture;
}

[System.Serializable]
public class serializableTextureAsset {
    public string fileName;
    public string fileHash;
}