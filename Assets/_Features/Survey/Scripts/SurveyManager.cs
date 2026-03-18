using QuestionnaireToolkit.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SurveySystem;
using System;

public class SurveyManager : Singleton<SurveyManager>, IInitializationListener {
    
    public GameObject SurveyBuildingManagerPrefab;

    SurveyBuildingManager _buildingManagerInstance;
    string _surveyJsonData;

    void Start() {
        if(_buildingManagerInstance == null && SceneLoadingManager.Instance != null)
             _buildingManagerInstance = SceneLoadingManager.Instance
            .InstantiateObjectInScene(SurveyBuildingManagerPrefab)
            .GetComponent<SurveyBuildingManager>();
    }

    public void OnSceneInitialized() {
        if (_buildingManagerInstance == null)
            _buildingManagerInstance = SceneLoadingManager.Instance
            .InstantiateObjectInScene(SurveyBuildingManagerPrefab)
            .GetComponent<SurveyBuildingManager>();
    }

    public void EnterSurveyBuilding() {
        _buildingManagerInstance.EnterSurveyBuilding();
        DownloadSurveyData();
    }

    public void StartSurveyRuntime() {

    }

    public void ExitSurveyBuilding() {
        MainManagerBase.Instance.ChangeState(ProjectState.Freecam);
    }

    public void CreateNewQuestionnare() {
       
    }

    public void SetSurveyData(string jsonString) {
        _surveyJsonData = jsonString;
    }

    #region Serialization

    public void SerializeSurvey() {

    }

    public void SerializeAnswers() {

    }

    public void DeserializeEditor(string SurveyJson) {
        _buildingManagerInstance.DeserializeSurvey(SurveyJson);
    }

    public void DeserializeViewing(string SurveyJson) {

    }

    #endregion

    // Upload/Download to server, maybe should be handled elsewhere

    public void DownloadSurveyData() {
        ServerCommunicationManager.Instance.StartSurveyDownload(ProjectManager.Instance.SelectedProject.ProjectName, (success, data) => {
            _surveyJsonData = data;
            print(_surveyJsonData);
        });
    }

    public void UploadSurveyData() {
        ServerCommunicationManager.Instance.StartSurveyUpload(_surveyJsonData, ProjectManager.Instance.SelectedProject.ProjectName);
    }
}
