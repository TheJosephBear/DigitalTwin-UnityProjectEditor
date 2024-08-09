using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;

public class EditorHUDui : UIBehaviour {

    public GameObject canvas;

    public override void Hide() {
        canvas.SetActive(false);
    }

    public override void Show() {
        canvas.SetActive(true);
    }

    public void UploadMap() {
        AudioManager.Instance.PlaySound(SoundType.click);
        FileBrowser.ShowLoadDialog(OnFileSelectedMap, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void SpawnMap() {
        ObjectUploadingManager.Instance.SpawnMap();
    }

    public void UploadDecoration() {
        AudioManager.Instance.PlaySound(SoundType.click);
        FileBrowser.ShowLoadDialog(OnFileSelectedObject, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void UploadDecorationVariant() {
        AudioManager.Instance.PlaySound(SoundType.click);
        FileBrowser.ShowLoadDialog(OnFileSelectedObject, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void AddNewDecoration() {
        AudioManager.Instance.PlaySound(SoundType.click);
        ObjectUploadingManager.Instance.CreateNewDecoration();
    }

    void OnFileSelectedMap(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                GameObject loadingObject = FileLoading.Instance.LoadModel(path);
                ObjectUploadingManager.Instance.UploadMap(loadingObject);
            }
        }
    }

    void OnFileSelectedObject(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                GameObject loadingObject = FileLoading.Instance.LoadModel(path);
                ObjectUploadingManager.Instance.UploadNewDecorationModel(loadingObject);
                Destroy(loadingObject);
            }
        }
    }

}
