using System;
using UnityEngine;
using UnityEngine.UI;

public class ModelUploadUI : UIBehaviour {

    // Callback to return the created ModelAsset
    public Action<ModelAsset> OnFilesSubmitted;

    public void Initialize(Action<ModelAsset> callback) {
        OnFilesSubmitted = callback;
    }

    /// <summary>
    /// Opens the file dialog and returns the created asset via callback
    /// </summary>
    public void OnSelectFiles() {
#if UNITY_EDITOR
        FileBrowserManager.Instance.ShowLoadDialogDebugMultiFile(
            files => HandleFilesSelected(files),
            "obj, mtl, png, jpg",
            multipleSelection: true
        );
#else
        FileBrowserManager.Instance.ShowLoadDialog(
            files => HandleFilesSelected(files+),
            "obj, mtl, png, jpg",
            multipleSelection: true
        );
#endif
    }

    private void HandleFilesSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files == null || files.Length == 0) {
            OnFilesSubmitted?.Invoke(null);
            return;
        }

        // Create the asset from selected files
        ModelAsset asset = AssetManager.Instance.CreateNewAssetFromFiles(files);

        // Invoke callback
        OnFilesSubmitted?.Invoke(asset);
        this.gameObject.SetActive(false);
    }

}

/*
 
FileLoadingManager.OnSelectFiles(asset => {
            
}); 

 */
