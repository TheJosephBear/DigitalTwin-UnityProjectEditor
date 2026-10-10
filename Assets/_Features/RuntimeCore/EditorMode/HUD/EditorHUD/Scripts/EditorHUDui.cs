using UnityEngine;

public class EditorHUDui : UIBehaviour, IProjectSettingsListener {

    public UISwitcher.UISwitcher CameraCollisionToggleRef;
    public GameObject MultiviewButton;
    public GameObject FPSButton;
    public GameObject SurveyButton;

    public override void Show() {
        base.Show();
    //    GetComponent<DecorationUI>().ToggleVariantUI(false);
        UIManager.Instance.SetRaycasterFromLatestUI();
    }

    public void OnSettingsChanged(ProjectSettingsSerializable settings) {
        MultiviewButton.SetActive(settings.multiviewAllowed);
        FPSButton.SetActive(settings.firstPersonAllowed);
        SurveyButton.SetActive(settings.surveyAllowed);
    }

    #region Button visual event change

    private void OnEnable() {
        ProjectSettingsManager.Instance.AddObserver(this);
        /*  GlobalSettings.OnSettingsLoadedOrChanged += HandleSettingsUpdated;

          if (GlobalSettings.Instance != null && GlobalSettings.Instance.IsLoaded) {
              HandleSettingsUpdated(GlobalSettings.Instance.Data);
          }*/
    }

    private void OnDisable() {
        ProjectSettingsManager.Instance.AddObserver(this);
        //  GlobalSettings.OnSettingsLoadedOrChanged -= HandleSettingsUpdated;
    }
    /*
    private void HandleSettingsUpdated(GlobalSettings.SettingsData data) {
        CameraCollisionToggleRef.SetWithoutNotify(data.IsCameraCollisionOn);
    }

    public void OnCollisionToggleChangedByUser(bool toggleOn) {
        GlobalSettings.Instance.Set(data => data.IsCameraCollisionOn = toggleOn);
    }
    */
    #endregion

    #region ButtonFunctions

    public void OnFPSSettings() {
        FPSManager.Instance.ToggleUI(true);
    }

    public void OnMapUpload() {
        // UIManager.Instance.ShowUI(UIType.MapUI);
        MapManager.Instance.ToggleMapUI(true);
    }

    public void OnGeoMap() {
        EditorManager.Instance.ChangeState(AppState.GeoLocalization);
        GeoMapManager.Instance.ActivateGeoLocalization(false);
    }

    public void OnTwoMapView() {
        EditorManager.Instance.ChangeState(AppState.MultiView);

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
        EditorManager.Instance.ChangeState(AppState.Survey);

    }

    public void OnOpenViewerTab() {
        ProjectManager.Instance.GetOpenedProjectExportURL((url) => {
            WebGLTabOpener.Instance.OpenLinkInNewTab(url);
        });
    }

    public void OnSave() {
        if (MainManagerBase.Instance is EditorManager editorMgr) {
            editorMgr.SaveProject();
        }
    }

    public void OnLeave() {
        if (MainManagerBase.Instance is EditorManager editorMgr) {
            editorMgr.ExitEditor((exitSuccess) => { }, save: false);
        }
    }

    public void OnColissionToggle(bool toggleOn) {
        CameraManager.Instance.ToggleCameraCollision(toggleOn);
    }

    public void OnSettings() {
        ProjectSettingsManager.Instance.ToggleUI(true);
    }

    #endregion

    /*
     
    public void onPrepnoutNaKameru() {
        EditorManager.Instance.ToggleCameraViewMode();
    }

    

    public void onDekorace() {
  //      UIManager.Instance.ShowUI(UIType.DecorationMain);
    }


    /*
    public void onUploadMap() {
        AudioManager.Instance.PlaySound(SoundType.click);
        FileBrowser.ShowLoadDialog(OnFileSelected, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
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

    

    void OnFileSelected(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                MapManager.Instance.UploadMapModel(AssetManager.Instance.CreateNewAssetFromFile(path));
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
