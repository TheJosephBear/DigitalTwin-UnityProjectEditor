using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for the main project managers (Editing and Viewing).
/// Provides shared service references and common functionality.
/// </summary>
public abstract class MainManagerBase : Singleton<MainManagerBase> {

    [Header("SceneType of this managers scene")]
    public SceneType SceneType;

    [Header("Service refferences")]
    public EditorProjectSerializer ProjectSerializer;
    public CameraManager EditorCameraManager;
    public MapManager MapManager;
    public GeoMapManager GeoMapManager;
    public ViewManager ViewManager;
    public MultiViewManager MultiViewManager;
    public SurveyManager SurveyManager;

    [Header("State parent")]
    public GameObject StateParent;
    List<StateBase> _stateScripts = new List<StateBase>();

    [HideInInspector]
    public ProjectState ActiveState { get; private set; }
    StateBase _activeStateScript;

    protected void Awake() {
        base.Awake();

        InitializeStateList();
    }

    protected virtual void InitializeStateList() {
        _stateScripts = StateParent.GetComponentsInChildren<StateBase>().ToList();
    }

    public virtual void ChangeState(ProjectState newState) {
        if (_activeStateScript != null) {
            // exit if trying to change to already selected state
            if (newState == _activeStateScript.State)
                return;

            // Exit previous state
            _activeStateScript.Exit();
        }

        _activeStateScript = _stateScripts.Find(baseClass => baseClass.State == newState);
        if (_activeStateScript == null) Debug.LogError($"State class not found for {newState}");
        ActiveState = newState;
        _activeStateScript.Enter();
    }

    public Type GetMainManagerType() {
        return this.GetType();
    }

}
