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

    /// <summary>
    /// Enters viewing mode and reports via callback if data was found.
    /// </summary>
    /// <param name="onResult">Callback returning true if survey has content, false if empty/failed.</param>
    public void EnterSurveyViewing(Action<bool> onResult = null, bool debug = false) {
        if (debug) {
            bool hasData = ValidateJsonContent(_surveyJsonData);
            _flowManager.EnterSurveyViewing(_surveyJsonData);
            onResult?.Invoke(hasData);
        } else {
            // We use a Coroutine so the calling thread isn't blocked
            StartCoroutine(DownloadAndEnterViewingRoutine(onResult));
        }
    }

    private IEnumerator DownloadAndEnterViewingRoutine(Action<bool> onResult) {
        string downloadedData = null;
        bool isDone = false;

        // Use your existing download logic
        yield return StartCoroutine(DownloadSurveyData(data => {
            downloadedData = data;
            _surveyJsonData = data; // Cache it locally
            isDone = true;
        }));

        bool hasContent = ValidateJsonContent(downloadedData);

        if (hasContent) {
            _flowManager.EnterSurveyViewing(downloadedData);
        }

        onResult?.Invoke(hasContent);
    }

    public void ExitSurvey() {
        // Save Builder/Viewer
        SaveSurvey();
        _flowManager.ExitSurvey();

        // Change state
        MainManagerBase.Instance.ChangeState(AppState.Freecam);
    }

    #endregion

    #region Server comm

    IEnumerator DownloadSurveyData(System.Action<string> onCompleted) {
        bool isDone = false;
        string result = null;

        ServerCommunicationManager.Instance.StartSurveyDownload(
            ProjectManager.Instance.SelectedProject.ProjectName,
            (success, data) => {
                result = data;
                isDone = true;
            });

        // Wait until callback fires
        yield return new WaitUntil(() => isDone);

        onCompleted?.Invoke(result);
    }

    public void DeserializeSurvey() {
        ServerCommunicationManager.Instance.StartSurveyDownload(ProjectManager.Instance.SelectedProject.ProjectName, (success, data) => {
            print("We did the " + success);
            print("We did the with the " + data);
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

    public void SetSurveyJson(string json, bool debug = false) {
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

    #region Helpers

    /// <summary>
    /// Internal check to see if the string contains actual survey questions.
    /// </summary>
    private bool ValidateJsonContent(string json) {
        return json != "";
        /*
        if (string.IsNullOrWhiteSpace(json) || json == "{}" || json == "[]") {
            return false;
        }

        try {
            var node = JSON.Parse(json);
            // Check if the questions array exists and has at least one entry
            if (node["questions"] != null) {
                return node["questions"].AsArray.Count > 0;
            }

            // If your JSON structure is just a top-level array
            if (node.AsArray != null) {
                return node.AsArray.Count > 0;
            }
        } catch {
            return false;
        }

        return false;
        */
    }

    #endregion
}
