using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

public class EditorHUDui : UIBehaviour {

    public GameObject canvas; 
    public Transform DecorationScrollViewPrefab;
    public Transform DecorationScrollViewInScene;
    public Button UIDecorationPrefabButton;
    public Button UIDecorationInSceneButton;

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
        ObjectUploadingManager.Instance.CreateNewDecorationPreset();
    }

    public void SaveProject() {
        ProjectSaver.Instance.SaveProject();
    }

    public void LoadProject() {
        ProjectSaver.Instance.LoadProject();
    }

    void OnFileSelectedMap(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                GameObject loadingObject = FileLoading.Instance.LoadModel(path);
                ObjectUploadingManager.Instance.UploadMap(loadingObject);
            } else {
                PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
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
            } else {
                PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
            }
        }
    }

    public void AddDecorationPrefabButton(Decoration decoration) {
        GameObject uiDecorButton = Instantiate(UIDecorationPrefabButton.gameObject);
        uiDecorButton.transform.SetParent(DecorationScrollViewPrefab);
        uiDecorButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        uiDecorButton.GetComponent<DecorationButton>().Initialize(decoration);
    }

    public void AddDecorationInSceneButton(GameObject decoration) {
        GameObject uiDecorButton = Instantiate(UIDecorationInSceneButton.gameObject);
        uiDecorButton.transform.SetParent(DecorationScrollViewInScene);
        uiDecorButton.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        uiDecorButton.GetComponent<DecorationInSceneButton>().Initialize(decoration);
    }

}
