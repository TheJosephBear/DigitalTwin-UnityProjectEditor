using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;
using System.Collections;

/// <summary>
/// Handles two separate kinds of images:
/// - Project images: belong to the opened project, identified by their content hash, saved with the project.
/// - Preview images: one per project, shown in the project list, keyed by project name. Used while no project is open.
/// </summary>
public class ImageManager : Singleton<ImageManager> {
    public GameObject ImageContainer;

    private List<TextureAsset> _loadedTextures = new List<TextureAsset>();
    private HashSet<string> _uploadedHashes = new HashSet<string>(); // Images known to be on the server (uploaded or downloaded)

    // Preview textures are reused in place when re-downloaded, so UI that already shows them stays valid
    private Dictionary<string, Texture2D> _previewTextures = new Dictionary<string, Texture2D>();

    #region File Dialog

    /// <summary>
    /// Lets the user pick an image and adds it to the opened project's images. Returns null when cancelled or invalid.
    /// </summary>
    public void AskForImageDialog(Action<TextureAsset> callback) {
        ShowImageFileDialog(file => callback?.Invoke(file != null ? AddProjectImage(file.data, file.fileInfo.name) : null));
    }

    /// <summary>
    /// Lets the user pick a preview image. Nothing is stored or uploaded; pass the result to UploadPreviewImage to apply it.
    /// The caller owns the returned texture and should destroy it if the image isn't used.
    /// </summary>
    public void AskForPreviewImageDialog(Action<PendingPreviewImage> callback) {
        ShowImageFileDialog(file => {
            if (file == null) {
                callback?.Invoke(null);
                return;
            }

            Texture2D tex = LoadTexture(file.data);
            callback?.Invoke(tex != null ? new PendingPreviewImage { Data = file.data, FileName = file.fileInfo.name, Texture = tex } : null);
        });
    }

    private void ShowImageFileDialog(Action<FrostweepGames.Plugins.WebGLFileBrowser.File> onSelected) {
        FileBrowserManager.Instance.ShowLoadDialog(files => {
            if (files != null && files.Length > 0 && files[0]?.data != null) {
                onSelected(files[0]);
            } else {
                if (MessageDisplayManager.Instance != null)
                    MessageDisplayManager.Instance.ShowMessage("Prosím vyberte soubor typu .png/.jpg");
                onSelected(null);
            }
        },
        filterExtensions: "png, jpg, jpeg",
        multipleSelection: false);
    }

    #endregion

    #region Project Images

    private const int RotatedJpgQuality = 90;

    /// <summary>
    /// Turns a project image by 90° clockwise. Images are identified by their content, so the result is a new image
    /// (uploaded like a newly picked one) and the caller replaces the old ID with it. Returns null if it can't be turned.
    /// </summary>
    public TextureAsset RotateProjectImage(string imageId) {
        TextureAsset source = GetTextureAssetByID(imageId);
        if (source == null || source.Texture == null) return null;

        Texture2D rotated = ImageOrientationUtils.Rotated90Clockwise(source.Texture);
        if (rotated == null) {
            Debug.LogError($"Failed to rotate image: {source.FileName}");
            return null;
        }

        // Turned from the decoded pixels, so repeated turns don't add up JPEG losses within a session
        bool isPng = string.Equals(Path.GetExtension(source.FileName), ".png", StringComparison.OrdinalIgnoreCase);
        byte[] data = isPng ? rotated.EncodeToPNG() : rotated.EncodeToJPG(RotatedJpgQuality);
        return AddProjectImage(data, source.FileName, rotated);
    }

    // decoded: the texture of the data when the caller already has it, owned by the manager from then on
    private TextureAsset AddProjectImage(byte[] data, string fileName, Texture2D decoded = null) {
        if (data == null) {
            if (decoded != null) Destroy(decoded);
            return null;
        }

        string fileHash = GetFileHash(data);
        TextureAsset existing = _loadedTextures.Find(x => x != null && x.FileHash == fileHash);
        if (existing != null) {
            if (decoded != null) Destroy(decoded);
            return existing;
        }

        Texture2D tex = decoded != null ? decoded : LoadTexture(data);
        if (tex == null) {
            Debug.LogError($"Failed to decode image: {fileName}");
            return null;
        }

        // Kept on disk so the upload doesn't need the bytes in memory
        string extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension)) extension = ".png";
        string localPath = Path.Combine(Application.persistentDataPath, fileHash + extension);
        try {
            File.WriteAllBytes(localPath, data);
        } catch (Exception e) {
            Debug.LogError($"Failed to save file to persistent path: {e.Message}");
        }

        GameObject go = new GameObject($"Tex_{fileName}");
        if (ImageContainer != null) go.transform.parent = ImageContainer.transform;

        TextureAsset asset = go.AddComponent<TextureAsset>();
        asset.ID = fileHash;
        asset.FileName = fileName;
        asset.FileHash = fileHash;
        asset.Texture = tex;
        asset.LocalPersistentPath = localPath;

        _loadedTextures.Add(asset);
        return asset;
    }

    public TextureAsset GetTextureAssetByID(string ID) {
        if (string.IsNullOrEmpty(ID)) return null;
        return _loadedTextures.Find(x => x != null && (x.ID == ID || x.FileHash == ID));
    }

    public void UploadImagesToServer(string projectName) {
        foreach (TextureAsset asset in _loadedTextures) {
            UploadImage(asset, projectName);
        }
    }

    /// <summary>
    /// Uploads only the given images (by ID / hash) that aren't on the server yet.
    /// The survey is saved separately from the project, so its images would otherwise wait for the next project save.
    /// </summary>
    public void UploadImagesIfMissing(IEnumerable<string> imageIds, string projectName) {
        foreach (string id in imageIds) {
            TextureAsset asset = GetTextureAssetByID(id);
            if (asset == null || _uploadedHashes.Contains(asset.FileHash)) continue;
            UploadImage(asset, projectName);
        }
    }

    private void UploadImage(TextureAsset asset, string projectName) {
        if (string.IsNullOrEmpty(asset.LocalPersistentPath) || !File.Exists(asset.LocalPersistentPath)) {
            Debug.LogWarning($"Attempted to upload {asset.FileName} but local file was missing.");
            return;
        }

        string hash = asset.FileHash;
        _uploadedHashes.Add(hash); // Marked right away so a second save doesn't send it again, reverted on failure
        ServerCommunicationManager.Instance.UploadImageToServer(
            asset.LocalPersistentPath,
            asset.FileName,
            projectName,
            hash,
            success => {
                if (!success) _uploadedHashes.Remove(hash);
            }
        );
    }

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
        string currentProject = ProjectManager.Instance.SelectedProject.ProjectName;

        if (data != null) {
            foreach (var serializableAsset in data) {
                bool isDone = false;
                DownloadProjectImage(serializableAsset.fileHash, serializableAsset.fileName, currentProject, success => isDone = true);
                yield return new WaitUntil(() => isDone);
            }
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Downloads images (by ID / hash) that aren't loaded yet, e.g. survey images that were uploaded
    /// with the survey but aren't listed in the saved project data.
    /// </summary>
    public void EnsureImagesLoaded(IEnumerable<string> imageIds, string projectName, Action onComplete = null) {
        StartCoroutine(EnsureImagesLoadedCoroutine(new List<string>(imageIds), projectName, onComplete));
    }

    private IEnumerator EnsureImagesLoadedCoroutine(List<string> imageIds, string projectName, Action onComplete) {
        foreach (string id in new HashSet<string>(imageIds)) {
            if (string.IsNullOrEmpty(id) || GetTextureAssetByID(id) != null) continue;

            bool isDone = false;
            DownloadProjectImage(id, $"{id}.png", projectName, success => isDone = true);
            yield return new WaitUntil(() => isDone);
        }

        onComplete?.Invoke();
    }

    private void DownloadProjectImage(string hash, string fileName, string projectName, Action<bool> onComplete) {
        if (GetTextureAssetByID(hash) != null) {
            onComplete?.Invoke(true);
            return;
        }

        ServerCommunicationManager.Instance.DownloadImageFromServer(projectName, hash, fileName, data => {
            if (data != null && AddProjectImage(data, fileName) != null) {
                _uploadedHashes.Add(hash);
                onComplete?.Invoke(true);
            } else {
                Debug.LogError($"Failed to download texture: {fileName}");
                onComplete?.Invoke(false);
            }
        });
    }

    /// <summary>
    /// Removes the opened project's images. Previews are kept for the project list.
    /// </summary>
    public void ClearManager() {
        foreach (var asset in _loadedTextures) {
            if (asset == null) continue;
            if (asset.Texture != null) Destroy(asset.Texture);
            Destroy(asset.gameObject);
        }
        _loadedTextures.Clear();
        _uploadedHashes.Clear();
    }

    #endregion

    #region Preview Images

    /// <summary>
    /// Returns the cached preview of a project, or null if it hasn't been downloaded or the project has none.
    /// </summary>
    public Texture2D GetPreviewTexture(string projectName) {
        if (string.IsNullOrEmpty(projectName)) return null;
        return _previewTextures.TryGetValue(projectName, out Texture2D tex) ? tex : null;
    }

    /// <summary>
    /// Downloads a project's preview from the server and caches it. Returns null when the project has no preview.
    /// </summary>
    public void DownloadPreviewImage(string projectName, Action<Texture2D> onComplete = null) {
        ServerCommunicationManager.Instance.DownloadPreviewImageFromServer(projectName, data => {
            onComplete?.Invoke(data != null ? StorePreview(projectName, data) : GetPreviewTexture(projectName));
        });
    }

    /// <summary>
    /// Uploads a new preview for a project. The cache is updated only once the server has accepted it.
    /// </summary>
    public void UploadPreviewImage(string projectName, PendingPreviewImage image, Action<bool> onComplete = null) {
        if (image == null || image.Data == null) {
            onComplete?.Invoke(false);
            return;
        }

        string extension = Path.GetExtension(image.FileName);
        if (string.IsNullOrEmpty(extension)) extension = ".png";

        ServerCommunicationManager.Instance.UploadPreviewImageToServer(image.Data, "preview" + extension, projectName, success => {
            if (success) {
                StorePreview(projectName, image.Data);
            } else if (MessageDisplayManager.Instance != null) {
                MessageDisplayManager.Instance.ShowMessage("Nahrání náhledového obrázku selhalo");
            }
            onComplete?.Invoke(success);
        });
    }

    // A generated preview carries this JPEG comment, so it is told apart from one the user uploaded
    private const string GeneratedPreviewComment = "DigitalTwin generated preview";
    private const int GeneratedPreviewQuality = 85;

    /// <summary>
    /// Replaces the project's preview with a generated one (render of the project), unless the user uploaded their own.
    /// The render is encoded right away, the caller can destroy it after the call.
    /// </summary>
    public void UpdateGeneratedPreview(string projectName, Texture2D render, Action<bool> onComplete = null) {
        if (string.IsNullOrEmpty(projectName) || render == null) {
            onComplete?.Invoke(false);
            return;
        }

        byte[] data = AddGeneratedPreviewMarker(render.EncodeToJPG(GeneratedPreviewQuality));

        ServerCommunicationManager.Instance.DownloadPreviewImageWithStatus(projectName, (existing, responseCode) => {
            // Replaced: no preview yet, or a generated one. A failed check keeps the preview, it may be the user's.
            bool hasPreview = IsImageData(existing);
            bool canReplace = hasPreview ? HasGeneratedPreviewMarker(existing) : existing != null || responseCode == 404;
            if (!canReplace) {
                onComplete?.Invoke(false);
                return;
            }

            ServerCommunicationManager.Instance.UploadPreviewImageToServer(data, "preview.jpg", projectName, success => {
                if (success) StorePreview(projectName, data);
                else Debug.LogWarning($"Generated preview of project {projectName} was not uploaded");
                onComplete?.Invoke(success);
            });
        });
    }

    private static bool IsImageData(byte[] data) {
        if (data == null || data.Length < 4) return false;
        bool isJpg = data[0] == 0xFF && data[1] == 0xD8;
        bool isPng = data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
        return isJpg || isPng;
    }

    // Comment segment (FF FE) inserted after the JFIF header, decoders ignore it
    private static byte[] AddGeneratedPreviewMarker(byte[] jpg) {
        int insertAt = 2; // After the start of image marker
        if (jpg.Length > 6 && jpg[2] == 0xFF && jpg[3] == 0xE0) insertAt = 4 + ((jpg[4] << 8) | jpg[5]);

        byte[] comment = System.Text.Encoding.ASCII.GetBytes(GeneratedPreviewComment);
        int segmentLength = comment.Length + 2; // Length field counts itself

        byte[] result = new byte[jpg.Length + 2 + segmentLength];
        Array.Copy(jpg, 0, result, 0, insertAt);
        result[insertAt] = 0xFF;
        result[insertAt + 1] = 0xFE;
        result[insertAt + 2] = (byte)(segmentLength >> 8);
        result[insertAt + 3] = (byte)segmentLength;
        Array.Copy(comment, 0, result, insertAt + 4, comment.Length);
        Array.Copy(jpg, insertAt, result, insertAt + 4 + comment.Length, jpg.Length - insertAt);
        return result;
    }

    // Walks the JPEG header segments up to the image data looking for the comment
    private static bool HasGeneratedPreviewMarker(byte[] data) {
        if (data == null || data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8) return false;

        int i = 2;
        while (i + 4 <= data.Length && data[i] == 0xFF) {
            byte marker = data[i + 1];
            int length = (data[i + 2] << 8) | data[i + 3];
            if (marker == 0xDA || length < 2) break; // Image data starts

            if (marker == 0xFE && i + 2 + length <= data.Length
                && System.Text.Encoding.ASCII.GetString(data, i + 4, length - 2) == GeneratedPreviewComment) {
                return true;
            }
            i += 2 + length;
        }
        return false;
    }

    /// <summary>
    /// Moves a cached preview to a new project name, so a renamed project keeps showing it without a re-download.
    /// </summary>
    public void RenamePreview(string oldProjectName, string newProjectName) {
        if (oldProjectName == newProjectName || !_previewTextures.TryGetValue(oldProjectName, out Texture2D tex)) return;

        _previewTextures.Remove(oldProjectName);
        RemovePreview(newProjectName);
        _previewTextures[newProjectName] = tex;
    }

    public void RemovePreview(string projectName) {
        if (string.IsNullOrEmpty(projectName) || !_previewTextures.TryGetValue(projectName, out Texture2D tex)) return;

        _previewTextures.Remove(projectName);
        if (tex != null) Destroy(tex);
    }

    /// <summary>
    /// Drops previews of projects that no longer exist.
    /// </summary>
    public void KeepOnlyPreviews(ICollection<string> projectNames) {
        foreach (string name in new List<string>(_previewTextures.Keys)) {
            if (!projectNames.Contains(name)) RemovePreview(name);
        }
    }

    private Texture2D StorePreview(string projectName, byte[] data) {
        if (_previewTextures.TryGetValue(projectName, out Texture2D tex) && tex != null) {
            tex.LoadImage(data);
            ImageOrientationUtils.ApplyExifOrientation(tex, data);
            return tex;
        }

        tex = LoadTexture(data);
        if (tex != null) _previewTextures[projectName] = tex;
        return tex;
    }

    #endregion

    #region Helpers

    private Texture2D LoadTexture(byte[] data) {
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(data)) {
            ImageOrientationUtils.ApplyExifOrientation(tex, data); // Phone photos would otherwise be rotated
            return tex;
        }

        Destroy(tex);
        return null;
    }

    private string GetFileHash(byte[] fileData) {
        using (var md5 = MD5.Create()) {
            byte[] hashBytes = md5.ComputeHash(fileData);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    #endregion
}

/// <summary>
/// A preview image picked by the user that hasn't been uploaded yet.
/// </summary>
public class PendingPreviewImage {
    public byte[] Data;
    public string FileName;
    public Texture2D Texture;
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
