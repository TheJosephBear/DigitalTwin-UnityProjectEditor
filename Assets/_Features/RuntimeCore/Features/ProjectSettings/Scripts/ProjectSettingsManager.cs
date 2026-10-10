using System;
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
        set => SetSetting(ref _settings.surveyAllowed, value);
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

    #endregion
}

[Serializable]
public class ProjectSettingsSerializable {
    public bool viewerCameraCollision;
    public bool editorCameraCollision;
    public bool multiviewAllowed;
    public bool surveyAllowed;
    public bool firstPersonAllowed;
}
