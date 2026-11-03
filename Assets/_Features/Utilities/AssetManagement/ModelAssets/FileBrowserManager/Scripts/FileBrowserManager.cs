using System;
using System.Collections;
using System.Collections.Generic;
using FrostweepGames.Plugins.WebGLFileBrowser;
using UnityEngine;
using TriLibCore;

/// <summary>
/// Provides a unified interface for opening a file browser and receiving user-selected files.
/// </summary>
public class FileBrowserManager : Singleton<FileBrowserManager> {
    /// <summary>
    /// Callback invoked when the user successfully selects files.
    /// </summary>
    Action<File[]> onFilesSelectedCallback;

    /// <summary>
    /// Initializes the file browser service and subscribes to WebGL file browser events.
    /// </summary>
    protected override void Awake() {
        base.Awake();
        WebGLFileBrowser.FilesWereOpenedEvent += FilesOpenedHandler;
        WebGLFileBrowser.FileOpenFailedEvent += FileOpenFailedHandler;
    }

    /// <summary>
    /// Cleans up event subscriptions when the service is destroyed.
    /// </summary>
    void OnDestroy() {
        WebGLFileBrowser.FilesWereOpenedEvent -= FilesOpenedHandler;
        WebGLFileBrowser.FileOpenFailedEvent -= FileOpenFailedHandler;
    }

    /// <summary>
    /// Opens a file selection dialog and registers a callback to receive the selected files.
    /// </summary>
    /// <param name="onFilesSelected">
    /// Callback invoked when the user completes file selection.
    /// </param>
    /// <param name="filterExtensions">
    /// File extensions to filter by (e.g. "obj", "png"). Defaults to "obj".
    /// </param>
    public void ShowLoadDialog(Action<File[]> onFilesSelected, string filterExtensions = "obj", bool multipleSelection = false) {
        onFilesSelectedCallback = onFilesSelected;
#if UNITY_EDITOR
        ShowEditorLoadDialog(filterExtensions);
#else
        WebGLFileBrowser.OpenFilePanelWithFilters(
            WebGLFileBrowser.GetFilteredFileExtensions(filterExtensions),
            multipleSelection
        );
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor only: the plugin's editor fallback refuses multiple selection, so the Unity file panel is used directly.
    /// The panel picks a single file even when multiple selection is requested. Cancelling does nothing, like in WebGL.
    /// </summary>
    void ShowEditorLoadDialog(string filterExtensions) {
        string extensions = (filterExtensions ?? "").Replace(" ", "").Replace(".", "");
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Vyberte soubor", "", new[] { "Files", extensions });
        if (string.IsNullOrEmpty(path)) return;

        byte[] data = System.IO.File.ReadAllBytes(path);
        File file = new File {
            fileInfo = new FileInfo {
                extension = System.IO.Path.GetExtension(path),
                name = System.IO.Path.GetFileNameWithoutExtension(path),
                fullName = System.IO.Path.GetFileName(path),
                path = path,
                length = data.Length,
                size = data.Length
            },
            data = data
        };
        onFilesSelectedCallback?.Invoke(new[] { file });
    }
#endif

    public void ShowLoadDialogDebugMultiFile(Action<FrostweepGames.Plugins.WebGLFileBrowser.File[]> onFilesSelected, string filterExtensions = "obj, mtl, png, jpg", bool multipleSelection = false) {
        onFilesSelectedCallback = onFilesSelected;
        WebGLFileBrowser.OpenFolderPanelWithFilters(
            WebGLFileBrowser.GetFilteredFileExtensions(filterExtensions)
        );
    }

    /// <summary>
    /// Handles successful file selection events from the WebGL file browser.
    /// </summary>
    /// <param name="files">
    /// Files selected by the user.
    /// </param>
    void FilesOpenedHandler(File[] files) {
        onFilesSelectedCallback?.Invoke(files);
    }

    /// <summary>
    /// Handles file selection failures and logs the reported error.
    /// </summary>
    /// <param name="error">
    /// Error message returned by the file browser.
    /// </param>
    void FileOpenFailedHandler(string error) {
        Debug.LogError($"File open failed: {error}");
    }
}
