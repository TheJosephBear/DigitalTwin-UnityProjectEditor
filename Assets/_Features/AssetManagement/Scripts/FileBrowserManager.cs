using System;
using System.Collections;
using System.Collections.Generic;
using FrostweepGames.Plugins.WebGLFileBrowser;
using UnityEngine;

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
        WebGLFileBrowser.FilesWereOpenedEvent += FilesWereOpenedEventHandler;
        WebGLFileBrowser.FileOpenFailedEvent += FileOpenFailedEventHandler;
    }

    /// <summary>
    /// Cleans up event subscriptions when the service is destroyed.
    /// </summary>
    void OnDestroy() {
        WebGLFileBrowser.FilesWereOpenedEvent -= FilesWereOpenedEventHandler;
        WebGLFileBrowser.FileOpenFailedEvent -= FileOpenFailedEventHandler;
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
    public void ShowLoadDialog(Action<File[]> onFilesSelected, string filterExtensions = "obj") {
        onFilesSelectedCallback = onFilesSelected;
        WebGLFileBrowser.OpenFilePanelWithFilters(
            WebGLFileBrowser.GetFilteredFileExtensions(filterExtensions),
            false
        );
    }

    /// <summary>
    /// Handles successful file selection events from the WebGL file browser.
    /// </summary>
    /// <param name="files">
    /// Files selected by the user.
    /// </param>
    void FilesWereOpenedEventHandler(File[] files) {
        onFilesSelectedCallback?.Invoke(files);
    }

    /// <summary>
    /// Handles file selection failures and logs the reported error.
    /// </summary>
    /// <param name="error">
    /// Error message returned by the file browser.
    /// </param>
    void FileOpenFailedEventHandler(string error) {
        Debug.LogError($"File open failed: {error}");
    }
}
