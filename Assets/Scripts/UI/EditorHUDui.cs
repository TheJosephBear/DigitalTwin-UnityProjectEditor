using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;
using TransformGizmos;

public class EditorHUDui : UIBehaviour {

    public override void Hide() {
        canvas.SetActive(false);
    }

    public override void Show() {
        canvas.SetActive(true);
        GetComponent<DecorationUI>().ToggleVariantUI(false);
        UImanager.Instance.SetRaycasterFromLatestUI();
    }

    public void onProjekt() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.ShowUI(UIType.ProjectSettings);
    }

    public void onDekorace() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.ShowUI(UIType.DecorationMain);
    }

    public void onMesto() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.ShowUI(UIType.Map);
    }

    public void onProstredi() {
        AudioManager.Instance.PlaySound(SoundType.click);
        print("not supported yet!");
        //    UImanager.Instance.ShowUI(UIType.ProjectSettings);
    }

    public void onUlozit() {
        AudioManager.Instance.PlaySound(SoundType.click);
        ProjectManager.Instance.SaveProject();
    }

    public void onNahrat() {
        AudioManager.Instance.PlaySound(SoundType.click);
        print("not supported yet!");
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

    public void onLeave() {
        StartCoroutine(UnloadEditing());
    }
    IEnumerator UnloadEditing() {
        ProjectManager.Instance.SaveProject();
        var task = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Editing);
        while (!task.IsCompleted) {
            yield return null;
        }
        ProjectManager.Instance.CloseProject();
        UImanager.Instance.HideUI(UIType.EditorHUD);
        UImanager.Instance.ShowUI(UIType.Projects);

    }
    /*
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
    */
}
