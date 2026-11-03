using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelUploadManager : Singleton<ModelUploadManager> {

    public ModelUploadUI ModelUploadUIPrefab;

    ModelUploadUI _instantiatedUI;
    Action<ModelAsset> _callback;
    bool _isBuilding;
    List<FrostweepGames.Plugins.WebGLFileBrowser.File> _files = new List<FrostweepGames.Plugins.WebGLFileBrowser.File>();

    public void AskForModel(Action<ModelAsset> callback) {
        if (_instantiatedUI == null) {
            _instantiatedUI = Instantiate(ModelUploadUIPrefab, transform);
        }

        _callback = callback;
        _instantiatedUI.gameObject.SetActive(true);
        _instantiatedUI.Initialize();
    }

    public void AddFiles(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        foreach (var file in files) {
            _files.Add(file);
            _instantiatedUI.AddFileNameToList(file.fileInfo.name);
        }
    }

    public void FinishUploading(Action<float> onProgress = null) {
        if (_isBuilding) return; // the Finish button can be clicked again while we wait for the loading screen frames
        StartCoroutine(FinishUploadingCoroutine(onProgress));
    }

    IEnumerator FinishUploadingCoroutine(Action<float> onProgress) {
        _isBuilding = true;
        UIManager.Instance.ShowUI(UIType.LoadingScreen);

        // WebGL is single threaded: the build below blocks the main thread, so let the loading screen
        // render and reach the screen before starting it
        yield return null;
        yield return null;

        ModelAsset asset = null;
        try {
            asset = AssetManager.Instance.CreateNewAssetFromFiles(_files.ToArray(), onProgress);
        } catch (Exception e) {
            Debug.LogException(e);
        }

        UIManager.Instance.HideUI(UIType.LoadingScreen);
        ExitUploading();
        _isBuilding = false;

        if (asset == null) {
            // Callers expect a valid asset (e.g. MapManager destroys the old map before using the new one)
            MessageDisplayManager.Instance.DisplayMessage("Model se nepodařilo nahrát.");
            yield break;
        }

        try {
            _callback?.Invoke(asset);
        } catch (Exception e) {
            Debug.LogException(e);
            MessageDisplayManager.Instance.DisplayMessage("Model se nepodařilo nahrát.");
        }
    }

    public void ExitUploading() {
        _files.Clear();
        _instantiatedUI.ClearFileList();
        HideUI();
    }

    public void HideUI() {
        if (_instantiatedUI != null) _instantiatedUI.gameObject.SetActive(false);
    }
}
