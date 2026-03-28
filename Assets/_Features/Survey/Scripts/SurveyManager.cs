using QuestionnaireToolkit.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SurveySystem;
using System;
using QuestionnaireToolkit.Scripts.SimpleJSON;

public class SurveyManager : Singleton<SurveyManager>, IInitializationListener {
    
    public GameObject SurveyBuildingManagerPrefab;

    SurveyFlowManager _flowManager;
    string _surveyJsonData;

    #region Init

    void Start() {
        if(_flowManager == null && SceneLoadingManager.Instance != null)
            _flowManager = SceneLoadingManager.Instance.InstantiateObjectInScene(SurveyBuildingManagerPrefab).GetComponent<SurveyFlowManager>();
    }

    public void OnSceneInitialized() {
        if (_flowManager == null)
            _flowManager = SceneLoadingManager.Instance.InstantiateObjectInScene(SurveyBuildingManagerPrefab).GetComponent<SurveyFlowManager>();
    }

    #endregion

    #region Enter/Exit

    public void EnterSurveyBuilding() {
        _flowManager.EnterSurveyBuilding();
        DeserializeSurvey();
    }

    public void EnterSurveyViewing() {
        _flowManager.EnterSurveyViewing();
        DeserializeSurvey();
    }

    public void ExitSurvey() {
        // Save Builder/Viewer
        SaveSurvey();
        _flowManager.ExitSurvey();

        // Change state
        MainManagerBase.Instance.ChangeState(ProjectState.Freecam);
    }

    #endregion

    #region Server comm

    public void DeserializeSurvey() {
        ServerCommunicationManager.Instance.StartSurveyDownload(ProjectManager.Instance.SelectedProject.ProjectName, (success, data) => {
            _surveyJsonData = data;
            if (_surveyJsonData != null) {
                _flowManager.DeserializeSurvey(_surveyJsonData);
            }
        });
    }

    public void UploadSurveyData() {
        ServerCommunicationManager.Instance.StartSurveyUpload(_surveyJsonData, ProjectManager.Instance.SelectedProject.ProjectName);
    }

    public void UploadSurveyAnswers() {

    }

    #endregion

    public void SaveSurvey() {
        _surveyJsonData = _flowManager.GetSurveyJsonData();
        UploadSurveyData();
    }

    public void SaveAnswers() {

    }
}
