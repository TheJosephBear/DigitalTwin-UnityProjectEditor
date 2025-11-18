using System.Collections;
using System.Collections.Generic;

public class EditorHUDui : UIBehaviour {

    public override void Show() {
        base.Show();
    //    GetComponent<DecorationUI>().ToggleVariantUI(false);
        UImanager.Instance.SetRaycasterFromLatestUI();
    }

    #region ButtonFunctions

    public void OnMapUpload() {
        UImanager.Instance.ShowUI(UIType.MapUI);
    }

    public void OnGeoMap() {
        EditorManager.Instance.ChangeEditorMode(EditorState.GeoLocalization);
    }

    public void OnTwoMapView() {
        EditorManager.Instance.ChangeEditorMode(EditorState.MultiView);

        /*
        //Togle zobrazeniMapy
        if (EditorManager.Instance.EditorModeCurrent == EditorMode.Freecam) {
            EditorManager.Instance.ChangeEditorMode(EditorMode.TwoMaps);
        } else if (EditorManager.Instance.EditorModeCurrent == EditorMode.TwoMaps) {
            EditorManager.Instance.ChangeEditorMode(EditorMode.Freecam);
        }
        */
    }

    public void OnSurvey() {
        EditorManager.Instance.ChangeEditorMode(EditorState.SurveyCreation);

    }

    public void OnSave() {
        ProjectManager.Instance.SaveProject();
    }

    public void OnLeave() {
        EditorManager.Instance.ExitEditor();
    }

    #endregion

    /*
     
    public void onPrepnoutNaKameru() {
        EditorManager.Instance.ToggleCameraViewMode();
    }

    

    public void onDekorace() {
  //      UImanager.Instance.ShowUI(UIType.DecorationMain);
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
