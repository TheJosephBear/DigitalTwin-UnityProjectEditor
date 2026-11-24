using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelUploadManager : Singleton<ModelUploadManager> {

    public ModelUploadUI ModelUploadUIPrefab;

    ModelUploadUI _instantiatedUI;
    Func<ModelAsset, IEnumerator> _callback;
    bool _isBuilding;
    List<FrostweepGames.Plugins.WebGLFileBrowser.File> _files = new List<FrostweepGames.Plugins.WebGLFileBrowser.File>();

    public void AskForModel(Action<ModelAsset> callback) {
        AskForModel(asset => {
            callback?.Invoke(asset);
            return null;
        });
    }

    /// <summary>
    /// Like <see cref="AskForModel(Action{ModelAsset})"/>, with a callback that takes frames (e.g. placing the model in the scene).
    /// It runs behind the loading screen, which hides once it ends.
    /// </summary>
    public void AskForModel(Func<ModelAsset, IEnumerator> callback) {
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
        LoadingScreenUI.SetStatus("Připravuji nahrávání…");

        // WebGL is single threaded: the steps below run on the main thread and yield frames in between,
        // so let the loading screen render and reach the screen before starting them
        yield return null;
        yield return null;

        AssetManager.PreparedModelFiles prepared = null;
        yield return CoroutineUtils.TryCoroutine(
            AssetManager.Instance.PrepareAssetFromFilesCoroutine(_files.ToArray(), result => prepared = result),
            Debug.LogException);

        // The files are on disk now and their bytes released. On WebGL the garbage collector only runs between frames,
        // so one passes before the model is parsed, the memory peak of the upload
        _files.Clear();
        yield return null;

        ModelAsset asset = null;
        yield return CoroutineUtils.TryCoroutine(
            AssetManager.Instance.BuildPreparedAssetCoroutine(prepared, result => asset = result),
            Debug.LogException);

        ExitUploading();

        if (asset == null) {
            // Callers expect a valid asset (e.g. MapManager destroys the old map before using the new one)
            UIManager.Instance.HideUI(UIType.LoadingScreen);
            _isBuilding = false;
            MessageDisplayManager.Instance.DisplayMessage("Model se nepodařilo nahrát.");
            yield break;
        }

        // Placing the model (instantiating, colliders) also takes frames, the loading screen stays until it's done
        bool callbackFailed = false;
        IEnumerator placing = null;
        try {
            placing = _callback?.Invoke(asset);
        } catch (Exception e) {
            Debug.LogException(e);
            callbackFailed = true;
        }
        if (placing != null) {
            yield return CoroutineUtils.TryCoroutine(placing, e => {
                Debug.LogException(e);
                callbackFailed = true;
            });
        }

        UIManager.Instance.HideUI(UIType.LoadingScreen);
        _isBuilding = false;
        if (callbackFailed) MessageDisplayManager.Instance.DisplayMessage("Model se nepodařilo nahrát.");
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
