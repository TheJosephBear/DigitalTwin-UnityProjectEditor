using System;
using System.Collections;
using System.Collections.Generic;
using FrostweepGames.Plugins.WebGLFileBrowser;
using UnityEngine;

public class FileBrowserManager : Singleton<FileBrowserManager> {
    
    Action<File[]> onFilesSelectedCallback;

    protected override void Awake() {
        base.Awake();
        WebGLFileBrowser.FilesWereOpenedEvent += FilesWereOpenedEventHandler;
        WebGLFileBrowser.FileOpenFailedEvent += FileOpenFailedEventHandler;
    }

    void OnDestroy() {
        WebGLFileBrowser.FilesWereOpenedEvent -= FilesWereOpenedEventHandler;
        WebGLFileBrowser.FileOpenFailedEvent -= FileOpenFailedEventHandler;
    }

    public void ShowLoadDialog(Action<File[]> onFilesSelected, string filterExtensions = "obj") {
        onFilesSelectedCallback = onFilesSelected;
        WebGLFileBrowser.OpenFilePanelWithFilters(
            WebGLFileBrowser.GetFilteredFileExtensions(filterExtensions),
            false 
        );
    }

    void FilesWereOpenedEventHandler(File[] files) {
        onFilesSelectedCallback?.Invoke(files);
    }

    void FileOpenFailedEventHandler(string error) {
        Debug.LogError($"File open failed: {error}");
    }
    
}
