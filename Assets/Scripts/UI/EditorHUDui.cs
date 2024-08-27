using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;
using TransformGizmos;

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

    public void onUploadMap() {
        AudioManager.Instance.PlaySound(SoundType.click);
        FileBrowser.ShowLoadDialog(OnFileSelectedMap, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }


    public void onAddNewDecoration() {
        AudioManager.Instance.PlaySound(SoundType.click);
        DecorationManager.Instance.CreateNewDecorationPreset();
    }

    public void onSaveProject() {
        AudioManager.Instance.PlaySound(SoundType.click);
        ProjectSaver.Instance.SaveProject();
    }

    public void onLoadProject() {
        AudioManager.Instance.PlaySound(SoundType.click);
        ProjectSaver.Instance.LoadProject();
    }

    public void onPositionGizmo() {
        AudioManager.Instance.PlaySound(SoundType.click);
        GizmoController.Instance.SelectMovement();
    }

    public void onRotationGizmo() {
        AudioManager.Instance.PlaySound(SoundType.click);
        GizmoController.Instance.SelectRotation();
    }

    public void onScaleGizmo() {
        AudioManager.Instance.PlaySound(SoundType.click);
        GizmoController.Instance.SelectScale();
    }

    void OnFileSelectedMap(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                MapManager.Instance.UploadMapModel(AssetManager.Instance.CreateNewAsset(path));
            } else {
                PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
            }
        }
    }

    public void AddDecorationPrefabButton(DecorationPreset decoration) {
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
