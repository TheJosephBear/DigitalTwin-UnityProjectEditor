using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;
using System.Collections;

public class ImageManager : Singleton<ImageManager> {
    public GameObject ImageContainer;
    private List<TextureAsset> _loadedTextures = new List<TextureAsset>();
    private List<TextureAsset> _previewTextures = new List<TextureAsset>();

    private Action<TextureAsset> _onTextureAssetLoadedCallback;
    private bool _processingPreview = false;
    private string _forcedProjectContext = "";

    #region Dialog Logic

    public void AskForImageDialog(Action<TextureAsset> callback, bool previewImage = false, string projectName = "") {
        _onTextureAssetLoadedCallback = callback;
        _processingPreview = previewImage;

        if (string.IsNullOrEmpty(projectName) && ProjectManager.Instance?.SelectedProject != null) {
            _forcedProjectContext = ProjectManager.Instance.SelectedProject.ProjectName;
        } else {
            _forcedProjectContext = projectName;
        }

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
        _processingPreview = false;
        _forcedProjectContext = "";
    }

    #endregion

    #region Asset Creation

    public TextureAsset CreateTextureAsset(FrostweepGames.Plugins.WebGLFileBrowser.File file) {
        if (file == null || file.data == null) return null;

        string currentProject = !string.IsNullOrEmpty(_forcedProjectContext) ? _forcedProjectContext : "UnknownProject";

        string fileHash = _processingPreview ? "PREVIEW" : GetFileHash(file.data);
        // Uses your preferred preview ID structure: Preview_ProjectName
        string assetID = _processingPreview ? $"Preview_{currentProject}" : fileHash;

        // --- 1. Manage Duplicates and Memory Optimization ---
        if (_processingPreview) {
            TextureAsset oldPreview = _previewTextures.Find(x => x.ID == assetID);
            if (oldPreview != null) {
                _previewTextures.Remove(oldPreview);
                if (oldPreview.Texture != null) Destroy(oldPreview.Texture);
                Destroy(oldPreview.gameObject);
            }
        } else {
            foreach (var existing in _loadedTextures) {
                if (existing.FileHash == fileHash) {
                    if (string.IsNullOrEmpty(existing.ID)) existing.ID = fileHash;
                    return existing;
                }
            }
        }

        // --- 2. Save File to Persistent Data Storage ---
        string extension = Path.GetExtension(file.fileInfo.name);
        if (string.IsNullOrEmpty(extension)) extension = ".png";

        string diskName = _processingPreview ? $"{currentProject}_preview" : fileHash;
        string localPath = Path.Combine(Application.persistentDataPath, diskName + extension);

        try {
            File.WriteAllBytes(localPath, file.data);
        } catch (Exception e) {
            Debug.LogError($"Failed to save file to persistent path: {e.Message}");
        }

        // --- 3. Build Unity Engine Wrapper Object Components ---
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(file.data)) return null;

        string gameObjectName = _processingPreview ? $"Preview_{currentProject}" : $"Tex_{file.fileInfo.name}";
        GameObject go = new GameObject(gameObjectName);
        if (ImageContainer != null) go.transform.parent = ImageContainer.transform;

        TextureAsset asset = go.AddComponent<TextureAsset>();
        asset.FileName = _processingPreview ? $"{currentProject}_preview.png" : file.fileInfo.name;
        asset.FileHash = fileHash;
        asset.ID = assetID;
        asset.Texture = tex;
        asset.LocalPersistentPath = localPath;

        // --- 4. Route Downstream (DECOUPLED FROM SERVER UPLOAD) ---
        if (_processingPreview) {
            _previewTextures.Add(asset);
            print($"Loaded preview texture locally with ID: {asset.ID}. (UPLOAD DEFERRED)");
        } else {
            _loadedTextures.Add(asset);
            print($"Added standard image asset with ID: {asset.ID}");

            if (MainManagerBase.Instance is EditorManager editorMan) {
            //    MessageDisplayManager.Instance.DisplayMessage("Image Manager save");
            //    editorMan.SaveProject();
            }
        }

        return asset;
    }

    #endregion

    #region Manual Explicit Server Uploads

    /// <summary>
    /// Call this explicitly when you are ready to send the preview to the server.
    /// </summary>
    public void UploadPreviewToServer(string projectName) {
        string targetID = $"Preview_{projectName}";
        TextureAsset preview = GetPreviewAssetByID(targetID);

        if (preview != null) {
            if (!string.IsNullOrEmpty(preview.LocalPersistentPath) && File.Exists(preview.LocalPersistentPath)) {
                ServerCommunicationManager.Instance.UploadPreviewImageToServer(
                    preview.LocalPersistentPath,
                    preview.FileName,
                    projectName,
                    "PREVIEW" // Passing "PREVIEW" matching your original logic string layout
                );
            } else {
                Debug.LogError($"Upload failed: Local file missing at {preview.LocalPersistentPath}");
            }
        } else {
            Debug.LogWarning($"No preview asset found in memory matching ID: {targetID}");
        }
    }

    public void UploadImagesToServer(string projectName) {
        foreach (TextureAsset asset in _loadedTextures) {
            if (!string.IsNullOrEmpty(asset.LocalPersistentPath) && File.Exists(asset.LocalPersistentPath)) {
                ServerCommunicationManager.Instance.UploadImageToServer(
                    asset.LocalPersistentPath,
                    asset.FileName,
                    projectName,
                    asset.FileHash
                );
            } else {
                Debug.LogWarning($"Attempted to upload {asset.FileName} but local file was missing.");
            }
        }
    }

    #endregion

    #region Serialization & Deserialization

    public List<serializableTextureAsset> SerializeTextureList() {
        List<serializableTextureAsset> list = new List<serializableTextureAsset>();
        foreach (var asset in _loadedTextures) {
            list.Add(new serializableTextureAsset {
                fileName = asset.FileName,
                fileHash = asset.FileHash
            });
        }
        return list;
    }

    public void Deserialize(List<serializableTextureAsset> data, Action onComplete = null) {
        StartCoroutine(DeserializeTexturesCoroutine(data, onComplete));
    }

    private IEnumerator DeserializeTexturesCoroutine(List<serializableTextureAsset> data, Action onComplete) {
        string currentProject = (ProjectManager.Instance?.SelectedProject != null) ? ProjectManager.Instance.SelectedProject.ProjectName : "UnknownProject";

        bool previewDone = false;
        DownloadPreviewImage(currentProject, success => {
            previewDone = true;
        });
        yield return new WaitUntil(() => previewDone);

        foreach (var serializableAsset in data) {
            bool isDone = false;

            DownloadTextureAsset(
                serializableAsset.fileHash,
                serializableAsset.fileName,
                currentProject,
                success => {
                    isDone = true;
                });

            yield return new WaitUntil(() => isDone);
        }

        onComplete?.Invoke();
    }

    public void DownloadPreviewImage(string projectName, Action<bool> onComplete = null) {
        string targetID = $"Preview_{projectName}";
        TextureAsset existingPreview = GetPreviewAssetByID(targetID);
        if (existingPreview != null) {
            _previewTextures.Remove(existingPreview);
            if (existingPreview.Texture != null) Destroy(existingPreview.Texture);
            Destroy(existingPreview.gameObject);
        }

        ServerCommunicationManager.Instance.DownloadPreviewImageFromServer(projectName, data => {
            if (data != null) {
                _processingPreview = true;
                _forcedProjectContext = projectName;

                FrostweepGames.Plugins.WebGLFileBrowser.File simulatedFile = new FrostweepGames.Plugins.WebGLFileBrowser.File {
                    data = data,
                    fileInfo = new FrostweepGames.Plugins.WebGLFileBrowser.FileInfo { name = $"{projectName}_preview.png" }
                };

                CreateTextureAsset(simulatedFile);
                _processingPreview = false;
                _forcedProjectContext = "";
                onComplete?.Invoke(true);
            } else {
                Debug.LogWarning($"No preview image found on server for project: {projectName}");
                onComplete?.Invoke(false);
            }
        });
    }

    private void DownloadTextureAsset(string hash, string fileName, string projectName, Action<bool> onComplete) {
        foreach (var asset in _loadedTextures) {
            if (asset.FileHash == hash) {
                onComplete?.Invoke(true);
                return;
            }
        }

        ServerCommunicationManager.Instance.DownloadImageFromServer(
            projectName,
            hash,
            fileName,
            data => {
                if (data != null) {
                    FrostweepGames.Plugins.WebGLFileBrowser.File simulatedFile = new FrostweepGames.Plugins.WebGLFileBrowser.File {
                        data = data,
                        fileInfo = new FrostweepGames.Plugins.WebGLFileBrowser.FileInfo { name = fileName }
                    };
                    CreateTextureAsset(simulatedFile);
                    onComplete?.Invoke(true);
                } else {
                    Debug.LogError($"Failed to download texture: {fileName}");
                    onComplete?.Invoke(false);
                }
            });
    }

    #endregion

    #region Accessors & Maintenance

    public void ClearManager() {
        foreach (var asset in _loadedTextures) {
            if (asset.Texture != null) Destroy(asset.Texture);
            Destroy(asset.gameObject);
        }
        _loadedTextures.Clear();

        foreach (var preview in _previewTextures) {
            if (preview.Texture != null) Destroy(preview.Texture);
            Destroy(preview.gameObject);
        }
        _previewTextures.Clear();
    }

    // UNIFIED LOOKUP: Automatically checks standard assets first, then fallback-searches previews by ID
    public TextureAsset GetTextureAssetByID(string ID) {
        TextureAsset standardAsset = _loadedTextures.Find(x => x.ID == ID);
        if (standardAsset != null) return standardAsset;

        return _previewTextures.Find(x => x.ID == ID);
    }

    public TextureAsset GetPreviewAssetByProject(string projectName) {
        return _previewTextures.Find(x => x.ID == $"Preview_{projectName}");
    }

    public TextureAsset GetPreviewAssetByID(string ID) {
        return _previewTextures.Find(x => x.ID == ID);
    }

    #endregion

    #region Helpers

    private string GetFileHash(byte[] fileData) {
        using (var md5 = MD5.Create()) {
            byte[] hashBytes = md5.ComputeHash(fileData);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    #endregion

    /// <summary>
    /// Takes any loaded texture asset and registers a clone of it as the official preview image for a project.
    /// </summary>
    public void SetAssetAsProjectPreview(TextureAsset sourceAsset, string projectName) {
        if (sourceAsset == null || sourceAsset.Texture == null) {
            Debug.LogError("SetAssetAsProjectPreview err: Source asset or texture is null!");
            return;
        }

        string targetID = $"Preview_{projectName}";

        // 1. Memory Cleanup: Wipe out any existing preview asset cached under this project ID
        TextureAsset oldPreview = _previewTextures.Find(x => x.ID == targetID);
        if (oldPreview != null) {
            _previewTextures.Remove(oldPreview);
            if (oldPreview.Texture != null) Destroy(oldPreview.Texture);
            Destroy(oldPreview.gameObject);
        }

        // 2. Clone Structure: Create a new detached GameObject container for our preview asset layer
        GameObject go = new GameObject(targetID);
        if (ImageContainer != null) go.transform.parent = ImageContainer.transform;

        // 3. Populate Properties: Mirror properties from the source asset to the new preview component context
        TextureAsset projectPreview = go.AddComponent<TextureAsset>();
        projectPreview.ID = targetID;
        projectPreview.FileName = $"{projectName}_preview.png";
        projectPreview.FileHash = sourceAsset.FileHash;
        projectPreview.Texture = sourceAsset.Texture; // Points directly to the same texture pointer in GPU memory
        projectPreview.LocalPersistentPath = sourceAsset.LocalPersistentPath;

        // 4. Save to Runtime Cache: Register it into the internal array container
        _previewTextures.Add(projectPreview);
        Debug.Log($"Successfully assigned texture hash {sourceAsset.FileHash} as preview for project: {projectName}");
    }

}

[System.Serializable]
public class TextureAsset : MonoBehaviour {
    public string ID;
    public string FileName;
    public string FileHash;
    public Texture2D Texture;
    public string LocalPersistentPath; 
}

[System.Serializable]
public class serializableTextureAsset {
    public string id;
    public string fileName;
    public string fileHash;
}
