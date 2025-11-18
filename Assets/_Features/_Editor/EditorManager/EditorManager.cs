using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class EditorManager : Singleton<EditorManager> {
    /// <summary>
    /// Controls the state of the Editor
    /// </summary>

    // Services
    [Header("Service refferences")]
    public EditorCameraManager EditorCameraManager;
    public MapManager MapManager;
    public GeoMapManager GeoMapManager;
    public ViewManager ViewManager;
    public MultiViewManager MultiViewManager;
    public SurveyManager SurveyManager;

    [Header("State parent")]
    public GameObject StateParent;
    List<EditorStateBase> _editorStateScripts = new List<EditorStateBase>();

    [HideInInspector]
    public EditorState ActiveState { get; private set; }
    EditorStateBase _activeStateScript;

    protected override void Awake() {
        base.Awake();
        InitializeStateList();
    }

    public void ChangeEditorMode(EditorState newState) {
        if (_activeStateScript != null) {
            // exit if trying to change to already selected state
            if (newState == _activeStateScript.State)
                return;

            // Exit previous state
            _activeStateScript.Exit();
        }

        _activeStateScript = _editorStateScripts.Find(baseClass => baseClass.State == newState);
        if (_activeStateScript == null) Debug.LogError($"State class not found for {newState}");
        ActiveState = newState;
        _activeStateScript.Enter();
    }

    public void ExitEditor() {
        ProjectManager.Instance.CloseProject();
    }

    void InitializeStateList() {
        _editorStateScripts = StateParent.GetComponentsInChildren<EditorStateBase>().ToList();
    }
}

public enum EditorState {
    Initialization,
    Freecam,
    GeoLocalization,
    MultiView,
    ViewActive,
    SurveyCreation,
}