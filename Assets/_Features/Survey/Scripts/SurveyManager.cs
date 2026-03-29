using QuestionnaireToolkit.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SurveySystem;
using System;
using QuestionnaireToolkit.Scripts.SimpleJSON;
using System.Diagnostics;

public class SurveyManager : Singleton<SurveyManager>, IInitializationListener {
    
    public GameObject SurveyBuildingManagerPrefab;

    SurveyFlowManager _flowManager;
    string _surveyJsonData;
    string _responseJsonData;

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

    public void EnterSurveyBuilding(bool debug = false) {
        _flowManager.EnterSurveyBuilding();
        if (debug) {
            _flowManager.DeserializeSurvey(_surveyJsonData);
        } else {
            DeserializeSurvey();
        }
    }

    public void EnterSurveyViewing(bool debug = false) {
        _flowManager.EnterSurveyViewing();
        if (debug) {
            _flowManager.DeserializeSurvey(_surveyJsonData);
        } else {
            DeserializeSurvey();
        }
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

    public void SetSurveyJson(string json) {
        _surveyJsonData = json;
    }

    public void SaveSurvey() {
        _surveyJsonData = _flowManager.GetSurveyJsonData();
        UploadSurveyData();
    }

    public void SaveAnswers() {
        _responseJsonData = _flowManager.GetResponseJsonData();
        print(_responseJsonData);
    }
}
