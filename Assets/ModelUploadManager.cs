using System;
using UnityEngine;

public class ModelUploadManager : Singleton<ModelUploadManager> {

    public ModelUploadUI ModelUploadUIPrefab;
    ModelUploadUI _instantiatedUI;

    public void AskForModel(Action<ModelAsset> callback) {
        if (_instantiatedUI == null) {
            _instantiatedUI = Instantiate(ModelUploadUIPrefab, transform);
        }

        _instantiatedUI.gameObject.SetActive(true);
        _instantiatedUI.Initialize(callback);

    }
}
