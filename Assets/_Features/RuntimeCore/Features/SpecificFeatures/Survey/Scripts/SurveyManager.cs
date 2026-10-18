using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SurveySystem;
using System;

public class SurveyManager : Singleton<SurveyManager>, IInitializationListener {

    public GameObject SurveyBuildingManagerPrefab;

    SurveyFlowManager _flowManager;
    string _surveyJsonData;
    string _responseJsonData;
    string _savedSurveyJson; // Survey as it was last loaded or saved, used to detect unsaved changes
    int _surveyLoadId; // Bumped on every load and exit, a download finishing for an older one is ignored
    bool _isSurveyLoading;

    #region Init

    void Start() {
        if (_flowManager == null)
            _flowManager = CreateFlowManager();
    }

    public void OnSceneInitialized() {
        if (_flowManager == null)
            _flowManager = CreateFlowManager();
    }

    // Start can run while the previous scene (project list) is still the active one.
    // Putting the flow manager there destroys it on unload, so keep it in this manager's scene.
    SurveyFlowManager CreateFlowManager() {
        GameObject flowManagerObject = Instantiate(SurveyBuildingManagerPrefab);
        SceneManager.MoveGameObjectToScene(flowManagerObject, gameObject.scene);
        return flowManagerObject.GetComponent<SurveyFlowManager>();
    }

    #endregion

    #region Enter/Exit

    public void EnterSurveyBuilding(bool debug = false) {
        _flowManager.EnterSurveyBuilding();
        _savedSurveyJson = _flowManager.GetSurveyJsonData();
        if (debug) {
            _flowManager.DeserializeSurvey(_surveyJsonData);
            _savedSurveyJson = _flowManager.GetSurveyJsonData();
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
            // Survey images may be missing from the project's image list (survey is saved separately), load them first
            bool imagesLoaded = false;
            ImageManager.Instance.EnsureImagesLoaded(CollectSurveyImageIds(downloadedData), ProjectManager.Instance.SelectedProject.ProjectName, () => imagesLoaded = true);
            yield return new WaitUntil(() => imagesLoaded);

            _flowManager.EnterSurveyViewing(downloadedData);
        }

        onResult?.Invoke(hasContent);
    }

    public void ExitSurvey(bool save = true) {
        // Save Builder/Viewer
        if (save) {
            SaveSurvey();
        } else if (_savedSurveyJson != null) {
            // Builder data persists between visits, throw away the unsaved edits
            _flowManager.RestoreSurvey(_savedSurveyJson);
        }

        // A download still in flight must not rebuild the destroyed editor UI
        _surveyLoadId++;
        _isSurveyLoading = false;
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
        string projectName = ProjectManager.Instance.SelectedProject.ProjectName;
        int loadId = ++_surveyLoadId;
        _isSurveyLoading = true;

        ServerCommunicationManager.Instance.StartSurveyDownload(projectName, (success, data) => {
            if (loadId != _surveyLoadId) return;

            _surveyJsonData = data;
            if (_surveyJsonData == null) {
                _isSurveyLoading = false;
                return;
            }

            // Images saved only with the survey aren't in the project's image list, load them before building the UI
            string json = _surveyJsonData;
            ImageManager.Instance.EnsureImagesLoaded(CollectSurveyImageIds(json), projectName, () => {
                if (loadId != _surveyLoadId) return;
                _isSurveyLoading = false;

                // The builder showed the previous state until now, edits made on it get replaced
                if (_flowManager.GetSurveyJsonData() != _savedSurveyJson)
                    ShowMessage("Změny provedené během načítání dotazníku byly nahrazeny verzí ze serveru.");

                _flowManager.DeserializeSurvey(json);
                _savedSurveyJson = _flowManager.GetSurveyJsonData();
            });
        });
    }

    public void UploadSurveyAnswers(Action<bool> onComplete = null) {
        string projectName = ProjectManager.Instance?.SelectedProject?.ProjectName;
        if (string.IsNullOrEmpty(projectName)) {
            UnityEngine.Debug.LogWarning("[SurveyManager] Cannot upload answers: ProjectName is empty");
            onComplete?.Invoke(false);
            return;
        }

        _responseJsonData = _flowManager != null ? _flowManager.GetResponseJsonData() : null;
        if (string.IsNullOrEmpty(_responseJsonData)) {
            UnityEngine.Debug.LogWarning("[SurveyManager] Cannot upload answers: response data is empty");
            onComplete?.Invoke(false);
            return;
        }

        UnityEngine.Debug.Log($"[SurveyManager] Uploading survey answers for project '{projectName}'");

        ServerCommunicationManager.Instance.StartSurveyResponseUpload(_responseJsonData, projectName, (success, response) => {
            if (success) {
                UnityEngine.Debug.Log("[SurveyManager] Survey answers uploaded successfully!");
            } else {
                UnityEngine.Debug.LogError($"[SurveyManager] Failed to upload survey answers: {response}");
            }
            onComplete?.Invoke(success);
        });
    }

    #endregion

    #region Submission flag

    const string SubmittedPrefsKeyPrefix = "SurveySubmitted_";

    static string GetSubmittedPrefsKey() {
        string projectName = ProjectManager.Instance?.SelectedProject?.ProjectName;
        return string.IsNullOrEmpty(projectName) ? null : SubmittedPrefsKeyPrefix + projectName;
    }

    /// <summary>
    /// True if this client already submitted the survey of the selected project (persists across page refresh).
    /// </summary>
    public bool IsSurveySubmitted() {
        string key = GetSubmittedPrefsKey();
        return key != null && PlayerPrefs.GetInt(key, 0) == 1;
    }

    public void MarkSurveySubmitted() {
        string key = GetSubmittedPrefsKey();
        if (key == null) return;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save(); // WebGL: flush to IndexedDB immediately, otherwise it may be lost on refresh
    }

#if UNITY_EDITOR
    // Debug only: allows filling the survey again in play mode
    public void ClearSurveySubmitted() {
        string key = GetSubmittedPrefsKey();
        if (key == null) return;
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
    }
#endif

    #endregion

    public void SetSurveyJson(string json, bool debug = false) {
        _surveyJsonData = json;
    }

    public void SaveSurvey() {
        // The builder still shows the previous state, uploading it would overwrite the survey on the server
        if (_isSurveyLoading) {
            ShowMessage("Dotazník se ještě načítá, zatím ho nelze uložit.");
            return;
        }

        string json = _flowManager.GetSurveyJsonData();
        _surveyJsonData = json;
        _savedSurveyJson = json;
        ServerCommunicationManager.Instance.StartSurveyUpload(json, ProjectManager.Instance.SelectedProject.ProjectName, (success, _) => {
            if (success) return;
            ShowMessage("Uložení dotazníku se nezdařilo.");
            // Not on the server, keep it unsaved unless a newer save already replaced it
            if (_savedSurveyJson == json) _savedSurveyJson = null;
        });

        // Images picked in the survey are otherwise uploaded only with the next project save
        if (MainManagerBase.Instance is EditorManager) {
            ImageManager.Instance.UploadImagesIfMissing(CollectSurveyImageIds(_surveyJsonData), ProjectManager.Instance.SelectedProject.ProjectName);
        }
    }

    /// <summary>
    /// True if the edited survey differs from the last loaded or saved version.
    /// </summary>
    public bool HasUnsavedSurveyChanges() {
        // While loading, edits are on the previous state and will be replaced, nothing to save
        if (_isSurveyLoading) return false;
        return _flowManager != null && _flowManager.GetSurveyJsonData() != _savedSurveyJson;
    }

    static void ShowMessage(string message) {
        if (MessageDisplayManager.Instance != null) MessageDisplayManager.Instance.ShowMessage(message);
    }

    /// <summary>
    /// IDs of all images used by the survey: intro image, question images and image choice options.
    /// </summary>
    static List<string> CollectSurveyImageIds(string surveyJson) {
        var ids = new List<string>();
        if (string.IsNullOrEmpty(surveyJson)) return ids;

        SurveySerializable survey;
        try {
            survey = JsonUtility.FromJson<SurveySerializable>(surveyJson);
        } catch (Exception e) {
            Debug.LogWarning($"[SurveyManager] Cannot read survey images: {e.Message}");
            return ids;
        }
        if (survey == null) return ids;

        void AddId(string id) {
            // Project preview is handled separately by ImageManager
            if (!string.IsNullOrEmpty(id) && !id.StartsWith("Preview_") && !ids.Contains(id)) ids.Add(id);
        }

        AddId(survey.ImageID);
        AddId(survey.ImageId);
        if (survey.Questions == null) return ids;

        foreach (SerializableQuestion question in survey.Questions) {
            if (question == null) continue;
            AddId(question.ImageID);
            AddId(question.ImageId);
            if (question.Answers == null) continue;

            foreach (AnswerBase answer in question.Answers) {
                if (answer is AnswerImage imageAnswer) AddId(imageAnswer.GetImageId());
            }
        }
        return ids;
    }

    public void SaveAnswers() {
        _responseJsonData = _flowManager != null ? _flowManager.GetResponseJsonData() : null;
        print(_responseJsonData);
    }

    #region Survey Validation

    /// <summary>
    /// Internal check to see if the string contains actual survey questions.
    /// </summary>
    private bool ValidateJsonContent(string json) {
        if (string.IsNullOrWhiteSpace(json)) {
            return false;
        }

        // 1. Sanitize whitespace and light cleanup
        string trimmed = json.Trim();

        // Check for trivial empty JSON objects/arrays
        if (trimmed == "{}" || trimmed == "[]" || trimmed.Length < 3) {
            return false;
        }

        // Must start and end with valid JSON brackets
        bool isObject = trimmed.StartsWith("{") && trimmed.EndsWith("}");
        bool isArray = trimmed.StartsWith("[") && trimmed.EndsWith("]");

        if (!isObject && !isArray) {
            return false;
        }

        try {
            string target = trimmed;

            // 2. Unwrap "survey_data" if present
            int surveyDataIndex = target.IndexOf("\"survey_data\"", StringComparison.OrdinalIgnoreCase);
            if (surveyDataIndex != -1) {
                int colonIndex = target.IndexOf(':', surveyDataIndex);
                if (colonIndex != -1) {
                    target = target.Substring(colonIndex + 1).Trim();
                }
            }

            // 3. Fallback: Check if the root target itself is a non-empty array
            if (target.StartsWith("[")) {
                // Check if there is actual content between [ and ]
                string arrayContent = target.Substring(1, target.LastIndexOf(']') - 1).Trim();
                return arrayContent.Length > 0;
            }

            // 4. Check for "Questions" or "questions" array in object
            int questionsIndex = target.IndexOf("\"Questions\"", StringComparison.OrdinalIgnoreCase);
            if (questionsIndex == -1) {
                questionsIndex = target.IndexOf("\"questions\"", StringComparison.OrdinalIgnoreCase);
            }

            if (questionsIndex != -1) {
                int colonIndex = target.IndexOf(':', questionsIndex);
                if (colonIndex != -1) {
                    string afterColon = target.Substring(colonIndex + 1).Trim();

                    // Make sure it starts an array
                    if (afterColon.StartsWith("[")) {
                        int closingBracketIndex = afterColon.IndexOf(']');
                        if (closingBracketIndex > 1) {
                            // Extract array contents and check if it has elements
                            string arrayContent = afterColon.Substring(1, closingBracketIndex - 1).Trim();
                            return !string.IsNullOrWhiteSpace(arrayContent) && arrayContent != "{}";
                        }
                    }
                }
            }
        } catch {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Synchronously checks if there is currently valid survey data cached locally in memory.
    /// </summary>
    public bool HasCachedSurvey() {
        return ValidateJsonContent(_surveyJsonData);
    }

    /// <summary>
    /// Asynchronously queries the server to check if a valid survey exists for the selected project.
    /// </summary>
    /// <param name="onResult">Callback returning true if a valid survey exists on the server, false otherwise.</param>
    public void CheckHasValidSurvey(Action<bool> onResult) {
        if (ProjectManager.Instance == null || ProjectManager.Instance.SelectedProject == null) {
            onResult?.Invoke(false);
            return;
        }

        StartCoroutine(CheckHasValidSurveyRoutine(onResult));
    }

    private IEnumerator CheckHasValidSurveyRoutine(Action<bool> onResult) {
        string downloadedData = null;
        bool isDone = false;

        // Start download routine and yield wait until completion flag is updated
        StartCoroutine(DownloadSurveyData(data => {
            downloadedData = data;
            isDone = true;
        }));

        // Wait until callback sets isDone to true
        yield return new WaitUntil(() => isDone);

        // Validate downloaded data
        bool isValid = ValidateJsonContent(downloadedData);

        // Cache when valid
        if (isValid) {
            _surveyJsonData = downloadedData;
        }

        onResult?.Invoke(isValid);
    }

    #endregion
}
