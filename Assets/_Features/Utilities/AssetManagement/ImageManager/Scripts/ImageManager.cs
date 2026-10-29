using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;
using System.Collections;

/// <summary>
/// Handles two separate kinds of images:
/// - Project images: belong to the opened project, saved with the project. Identified by the content hash of the
///   file they were added with; a losslessly turned image keeps its ID while its content changes.
/// - Preview images: one per project, shown in the project list, keyed by project name. Used while no project is open.
/// </summary>
public class ImageManager : Singleton<ImageManager> {
    public GameObject ImageContainer;

    private List<TextureAsset> _loadedTextures = new List<TextureAsset>();
    private HashSet<string> _uploadedIds = new HashSet<string>(); // Images known to be on the server (uploaded or downloaded)
    private HashSet<string> _changedIds = new HashSet<string>(); // Turned in place, the server still has the previous file

    /// <summary>True while an image was turned in place and its new file hasn't been uploaded yet.</summary>
    public bool HasUnsavedImageChanges => _changedIds.Count > 0;

    // Preview textures are reused in place when re-downloaded, so UI that already shows them stays valid
    private Dictionary<string, Texture2D> _previewTextures = new Dictionary<string, Texture2D>();

    #region File Dialog

    /// <summary>
    /// Lets the user pick an image and adds it to the opened project's images. Returns null when cancelled or invalid.
    /// </summary>
    public void AskForImageDialog(Action<TextureAsset> callback) {
        ShowImageFileDialog(file => callback?.Invoke(file != null ? AddProjectImage(file.data, PickedFileName(file)) : null));
    }

    // fileInfo.name has no extension, fullName does
    private static string PickedFileName(FrostweepGames.Plugins.WebGLFileBrowser.File file) {
        return !string.IsNullOrEmpty(file.fileInfo.fullName) ? file.fileInfo.fullName : file.fileInfo.name;
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
            callback?.Invoke(tex != null ? new PendingPreviewImage { Data = file.data, FileName = WithContentExtension(PickedFileName(file), file.data), Texture = tex } : null);
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
    /// Turns a project image by 90° clockwise and returns the image to show; the caller stores its ID.
    /// A lossless turn (new EXIF orientation of a JPEG, re-encoded PNG) replaces the image's file and keeps its ID,
    /// the server gets the new file with the next save. Re-encoding a JPEG loses quality, so then the original stays
    /// and the turned picture is a new image. Returns null if the image can't be turned.
    /// </summary>
    public TextureAsset RotateProjectImage(string imageId) {
        TextureAsset source = GetTextureAssetByID(imageId);
        if (source == null || source.Texture == null) return null;

        byte[] original = ReadLocalImageData(source);
        byte[] data = ImageOrientationUtils.RotateJpeg90ClockwiseByExif(original);
        bool isPng = original != null ? IsPngData(original) : HasExtension(source.FileName, ".png");

        if (data == null && !isPng) {
            // JPEG whose EXIF has no orientation tag, or its file is gone. Same pixels the new file decodes to
            Texture2D rotated = ImageOrientationUtils.Rotated90Clockwise(source.Texture);
            if (rotated == null) {
                Debug.LogError($"Failed to rotate image: {source.FileName}");
                return null;
            }
            return AddProjectImage(rotated.EncodeToJPG(RotatedJpgQuality), source.FileName, rotated);
        }

        // Turned in place, so everything showing the texture shows the turned picture
        if (!ImageOrientationUtils.Rotate90ClockwiseInPlace(source.Texture)) {
            Debug.LogError($"Failed to rotate image: {source.FileName}");
            return null;
        }
        ReplaceImageData(source, data ?? source.Texture.EncodeToPNG());
        return source;
    }

    // New content for an image that keeps its ID: the local copy is replaced and the file is uploaded again
    private void ReplaceImageData(TextureAsset asset, byte[] data) {
        string oldPath = asset.LocalPersistentPath;
        asset.FileHash = GetFileHash(data);
        asset.LocalPersistentPath = WriteLocalCopy(asset.FileHash, asset.FileName, data);

        if (!string.IsNullOrEmpty(oldPath) && oldPath != asset.LocalPersistentPath
            && !_loadedTextures.Exists(x => x != null && x.LocalPersistentPath == oldPath)) {
            try {
                File.Delete(oldPath);
            } catch (Exception e) {
                Debug.LogWarning($"Failed to delete replaced file {oldPath}: {e.Message}");
            }
        }

        _uploadedIds.Remove(asset.ID);
        _changedIds.Add(asset.ID);
    }

    private static byte[] ReadLocalImageData(TextureAsset asset) {
        if (string.IsNullOrEmpty(asset.LocalPersistentPath) || !File.Exists(asset.LocalPersistentPath)) return null;
        try {
            return File.ReadAllBytes(asset.LocalPersistentPath);
        } catch (Exception e) {
            Debug.LogWarning($"Failed to read {asset.FileName} for rotating: {e.Message}");
            return null;
        }
    }

    // decoded: the texture of the data when the caller already has it, owned by the manager from then on.
    // id: the image's ID on the server when downloading it; a picked image is found by, and named after, its content
    private TextureAsset AddProjectImage(byte[] data, string fileName, Texture2D decoded = null, string id = null) {
        if (data == null) {
            if (decoded != null) Destroy(decoded);
            return null;
        }

        fileName = WithContentExtension(fileName, data);
        string fileHash = GetFileHash(data);
        if (id == null) {
            TextureAsset existing = _loadedTextures.Find(x => x != null && x.FileHash == fileHash);
            if (existing != null) {
                if (decoded != null) Destroy(decoded);
                return existing;
            }

            // The ID is taken by an image that was turned in place since, so this picture needs another one
            id = GetTextureAssetByID(fileHash) == null ? fileHash : Guid.NewGuid().ToString("N");
        }

        Texture2D tex = decoded != null ? decoded : LoadTexture(data);
        if (tex == null) {
            Debug.LogError($"Failed to decode image: {fileName}");
            return null;
        }

        GameObject go = new GameObject($"Tex_{fileName}");
        if (ImageContainer != null) go.transform.parent = ImageContainer.transform;

        TextureAsset asset = go.AddComponent<TextureAsset>();
        asset.ID = id;
        asset.FileName = fileName;
        asset.FileHash = fileHash;
        asset.Texture = tex;
        asset.LocalPersistentPath = WriteLocalCopy(fileHash, fileName, data);

        _loadedTextures.Add(asset);
        return asset;
    }

    // Kept on disk so the upload doesn't need the bytes in memory
    private static string WriteLocalCopy(string fileHash, string fileName, byte[] data) {
        string extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension)) extension = ".png";
        string localPath = Path.Combine(Application.persistentDataPath, fileHash + extension);
        try {
            File.WriteAllBytes(localPath, data);
        } catch (Exception e) {
            Debug.LogError($"Failed to save file to persistent path: {e.Message}");
        }
        return localPath;
    }

    public TextureAsset GetTextureAssetByID(string ID) {
        if (string.IsNullOrEmpty(ID)) return null;
        return _loadedTextures.Find(x => x != null && x.ID == ID);
    }

    public void UploadImagesToServer(string projectName) {
        foreach (TextureAsset asset in _loadedTextures) {
            UploadImage(asset, projectName);
        }
    }

    /// <summary>
    /// Uploads only the given images (by ID) that aren't on the server yet, or were turned since.
    /// The survey is saved separately from the project, so its images would otherwise wait for the next project save.
    /// </summary>
    public void UploadImagesIfMissing(IEnumerable<string> imageIds, string projectName) {
        foreach (string id in imageIds) {
            TextureAsset asset = GetTextureAssetByID(id);
            if (asset == null || _uploadedIds.Contains(asset.ID)) continue;
            UploadImage(asset, projectName);
        }
    }

    // Stored on the server under the image's ID; the upload replaces the file a turned image had before
    private void UploadImage(TextureAsset asset, string projectName) {
        if (string.IsNullOrEmpty(asset.LocalPersistentPath) || !File.Exists(asset.LocalPersistentPath)) {
            Debug.LogWarning($"Attempted to upload {asset.FileName} but local file was missing.");
            return;
        }

        string id = asset.ID;
        string uploadedHash = asset.FileHash;
        _uploadedIds.Add(id); // Marked right away so a second save doesn't send it again, reverted on failure
        ServerCommunicationManager.Instance.UploadImageToServer(
            asset.LocalPersistentPath,
            asset.FileName,
            projectName,
            id,
            success => {
                if (!success) _uploadedIds.Remove(id);
                // Unless it was turned again meanwhile
                else if (asset != null && asset.FileHash == uploadedHash) _changedIds.Remove(id);
            }
        );
    }

    public List<serializableTextureAsset> SerializeTextureList() {
        List<serializableTextureAsset> list = new List<serializableTextureAsset>();
        foreach (var asset in _loadedTextures) {
            list.Add(new serializableTextureAsset {
                id = asset.ID,
                fileName = asset.FileName,
                fileHash = asset.ID // The key the image is stored and downloaded by, older projects read this field
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
            DownloadProjectImage(id, id, projectName, success => isDone = true); // Extension is added by content
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
            if (data != null && AddProjectImage(data, fileName, null, hash) != null) {
                _uploadedIds.Add(hash);
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
        _uploadedIds.Clear();
        _changedIds.Clear();
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
        return IsJpegData(data) || IsPngData(data);
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

    private static bool IsJpegData(byte[] data) {
        return data != null && data.Length >= 4 && data[0] == 0xFF && data[1] == 0xD8;
    }

    private static bool IsPngData(byte[] data) {
        return data != null && data.Length >= 4 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
    }

    private static bool HasExtension(string fileName, params string[] extensions) {
        string extension = Path.GetExtension(fileName ?? "");
        return Array.Exists(extensions, e => string.Equals(e, extension, StringComparison.OrdinalIgnoreCase));
    }

    // The file name gets the extension of what the data is: picked names may come without one,
    // and images downloaded without a name were once all called .png
    private static string WithContentExtension(string fileName, byte[] data) {
        string extension = IsPngData(data) ? ".png" : IsJpegData(data) ? ".jpg" : null;
        if (extension == null || string.IsNullOrEmpty(fileName)) return fileName;

        if (extension == ".png" ? HasExtension(fileName, ".png") : HasExtension(fileName, ".jpg", ".jpeg")) return fileName;
        string baseName = HasExtension(fileName, ".png", ".jpg", ".jpeg") ? Path.GetFileNameWithoutExtension(fileName) : fileName;
        return baseName + extension;
    }

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
