using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectSettingsManager: Singleton<ProjectSettingsManager> {

    public GameObject UIPrefab;

    [SerializeField]
    private ProjectSettingsSerializable _settings = new ProjectSettingsSerializable();

    private readonly List<IProjectSettingsListener> _listeners = new List<IProjectSettingsListener>();
    private ProjectSettingsUI _UIInstance;

    public void ToggleUI(bool toggleOn) {
        if (_UIInstance == null) {
            if (SceneLoadingManager.Instance != null) {
                _UIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(UIPrefab).GetComponent<ProjectSettingsUI>();
            } else {
                _UIInstance = Instantiate(UIPrefab).GetComponent<ProjectSettingsUI>();
            }

            _UIInstance.Initialize(this);
        }
        
        _UIInstance.gameObject.SetActive(toggleOn);
    }

    #region Observer Registry

    public void AddObserver(IProjectSettingsListener observer) {
        if (!_listeners.Contains(observer)) {
            _listeners.Add(observer);
            // Notify immediately upon registration so the module syncs with current state
            observer.OnSettingsChanged(_settings);
        }
    }

    public void RemoveObserver(IProjectSettingsListener observer) {
        _listeners.Remove(observer);
    }

    private void NotifyObservers() {
        for (int i = _listeners.Count - 1; i >= 0; i--) {
            _listeners[i]?.OnSettingsChanged(_settings);
        }
    }

    #endregion

    #region Properties (Getters / Setters)

    public bool ViewerCameraCollision {
        get => _settings.viewerCameraCollision;
        set => SetSetting(ref _settings.viewerCameraCollision, value);
    }

    public bool MultiviewAllowed {
        get => _settings.multiviewAllowed;
        set => SetSetting(ref _settings.multiviewAllowed, value);
    }

    public bool SurveyAllowed {
        get => _settings.surveyAllowed;
        set {
            _settings.surveyAllowedSet = true;
            SetSetting(ref _settings.surveyAllowed, value);
        }
    }

    public bool FirstPersonAllowed {
        get => _settings.firstPersonAllowed;
        set => SetSetting(ref _settings.firstPersonAllowed, value);
    }

    public bool EditorCameraCollision {
        get => _settings.editorCameraCollision;
        set => SetSetting(ref _settings.editorCameraCollision, value);
    }

    private void SetSetting<T>(ref T field, T value) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        NotifyObservers();
    }

    #endregion

    #region Serialization

    // Returns the direct state object for saving
    public ProjectSettingsSerializable Serialize() {
        return _settings;
    }

    // Overwrites the entire container at once during loading
    public void Deserialize(ProjectSettingsSerializable serializable) {
        if (serializable == null) return;

        _settings = serializable;
        NotifyObservers();
    }

    /// <summary>
    /// Projects saved before the survey toggle existed load with surveyAllowed == false even when they already have a survey.
    /// For those, enable the toggle if a valid survey exists on the server. Only a found survey is stored,
    /// so a failed download doesn't hide the survey for good (the check simply runs again on the next load).
    /// </summary>
    public IEnumerator DetectExistingSurveyCoroutine() {
        if (_settings.surveyAllowedSet || _settings.surveyAllowed) yield break;
        if (SurveyManager.Instance == null || ProjectManager.Instance?.SelectedProject == null) yield break;

        bool isDone = false;
        bool hasSurvey = false;
        SurveyManager.Instance.CheckHasValidSurvey(result => {
            hasSurvey = result;
            isDone = true;
        });
        yield return new WaitUntil(() => isDone);

        if (hasSurvey) {
            Debug.Log("[ProjectSettingsManager] Existing survey found in an older project, enabling the survey setting");
            SurveyAllowed = true;
        }
    }

    #endregion
}
