using System;
using System.Collections.Generic;
using UnityEngine;

public class ModelUploadManager : Singleton<ModelUploadManager> {

    public ModelUploadUI ModelUploadUIPrefab;

    ModelUploadUI _instantiatedUI;
    Action<ModelAsset> _callback;
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

    public void FinishUploading() {
        ModelAsset asset = AssetManager.Instance.CreateNewAssetFromFiles(_files.ToArray());
        _callback?.Invoke(asset);
        ExitUploading();
    }

    public void ExitUploading() {
        _instantiatedUI.ClearFileList();
        HideUI();
    }

    public void HideUI() {
        if (_instantiatedUI != null) _instantiatedUI.gameObject.SetActive(false);
    }
}
