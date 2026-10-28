using UnityEngine;

public class AssetSceneTester: MonoBehaviour {
    public void OnUpload() {
        ModelUploadManager.Instance.AskForModel(OnModelUploaded);
     //   FileBrowserManager.Instance.ShowLoadDialog(OnFileSelected);
    }

    void OnModelUploaded(ModelAsset aa) {
        aa.gameObject.SetActive(true);
    }
}
