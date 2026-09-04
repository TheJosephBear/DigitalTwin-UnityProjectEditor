using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelUploadUI : UIBehaviour {

    public GameObject FileTextPrefab;
    public Transform ScrollviewContentTransformRef;

    public void Initialize() {
        ClearFileList();
    }

    public void OnFinished() {
        ModelUploadManager.Instance.FinishUploading();
    }

    public void OnCancel() {
        //    ModelUploadManager.Instance.HideUI();
        ModelUploadManager.Instance.ExitUploading();
    }

    public void AddFileNameToList(string fileName) {
        GameObject go = Instantiate(FileTextPrefab, ScrollviewContentTransformRef);
        go.GetComponent<TextMeshProUGUI>().text = fileName;
    }

    public void ClearFileList() {
         Utilities.KillAllChildren(ScrollviewContentTransformRef);
    }

    /// <summary>
    /// Opens the file dialog and returns the created asset via callback
    /// </summary>
    public void OnSelectFiles() {
#if UNITY_EDITOR
        FileBrowserManager.Instance.ShowLoadDialogDebugMultiFile(
            files => HandleFilesSelected(files),
            FileLoadingManager.Instance.GetAllowedExtensionsString(),
            multipleSelection: true
        );
#else
        FileBrowserManager.Instance.ShowLoadDialog(
            files => HandleFilesSelected(files),
            FileLoadingManager.Instance.GetAllowedExtensionsString(),
            multipleSelection: true
        );
#endif
    }

    private void HandleFilesSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files == null || files.Length == 0) {
       //     OnFilesSubmitted?.Invoke(null);
            return;
        }

        ModelUploadManager.Instance.AddFiles(files);

        /*
        // Create the asset from selected files
        ModelAsset asset = AssetManager.Instance.CreateNewAssetFromFiles(files);

        // Invoke callback
        OnFilesSubmitted?.Invoke(asset);
        this.gameObject.SetActive(false);
        */
    }

}

/*
 
FileLoadingManager.OnSelectFiles(asset => {
            
}); 

 */
