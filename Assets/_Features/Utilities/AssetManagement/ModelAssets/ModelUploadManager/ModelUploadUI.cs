using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelUploadUI : UIBehaviour {

    [Header("UI References")]
    public GameObject FileTextPrefab;
    public Transform ScrollviewContentTransformRef;

    [Header("Progress UI")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject progressContainer;

    public void Initialize() {
        ClearFileList();
        UpdateProgress(0f, false);
    }

    public void OnFinished() {
        ModelUploadManager.Instance.FinishUploading(progress => UpdateProgress(progress, true));
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
    /// Call this method to update the progress bar and percentage text.
    /// </summary>
    /// <param name="progress">Normalized value between 0.0 and 1.0</param>
    /// <param name="isVisible">Whether the progress UI should be visible.</param>
    public void UpdateProgress(float progress, bool isVisible = true) {
        if (progressContainer != null) {
            progressContainer.SetActive(isVisible);
        }

        if (progressBar != null) {
            progressBar.value = progress;
        }

        if (progressText != null) {
            progressText.text = $"Loading... {Mathf.RoundToInt(progress * 100f)}%";
        }
    }

    /// <summary>
    /// Opens the file dialog and returns the created asset via callback
    /// </summary>
    public void OnSelectFiles() {
        string extensions = FileLoadingManager.Instance.GetAllAllowedExtensionsString();
        extensions = extensions.Replace(".", "");

#if UNITY_EDITOR
        FileBrowserManager.Instance.ShowLoadDialogDebugMultiFile(
            files => HandleFilesSelected(files),
            extensions,
            multipleSelection: true
        );
#else
        FileBrowserManager.Instance.ShowLoadDialog(
            files => HandleFilesSelected(files),
            extensions,
            multipleSelection: true
        );
#endif
    }

    private void HandleFilesSelected(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files == null || files.Length == 0) {
       //     OnFilesSubmitted?.Invoke(null);
            return;
        }

     //   UpdateProgress(0f, true);
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
