using System;
using UnityEngine;

public class ViewMovingManager : MonoBehaviour {

    public GameObject ViewPointContextUIPrefab;

    CustomMovement _movementScript;
    ViewPoint _viewPointBeingMoved;
    ViewPointContextUI _viewPointContextUIInstance;
    Vector3 _originalPos;
    Quaternion _originalRot;
    string _originalName;
    bool _unsavedChanges = false;
    bool _movingActive = false;

    public bool HasUnsavedChanges => _unsavedChanges;

    void Awake() {
        _movementScript = GetComponent<CustomMovement>();
    }

    private void Update() {
        if (_unsavedChanges || !_movingActive) return;
        if(_viewPointBeingMoved.transform.position != _originalPos || _viewPointBeingMoved.transform.rotation != _originalRot) {
            ToggleUnsavedChanges(true);
        }
    }

    public void StartViewMoving(ViewPoint viewPointToMove) {
        _movingActive = true;
        _viewPointBeingMoved = viewPointToMove;
        _originalPos = _viewPointBeingMoved.transform.position;
        _originalRot = _viewPointBeingMoved.transform.rotation;
        _originalName = _viewPointBeingMoved.Name;
        // Start controlling it
        _movementScript.SetTarget(_viewPointBeingMoved.gameObject);
        // Show context UI
        if (_viewPointContextUIInstance == null)
            _viewPointContextUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointContextUIPrefab).GetComponent<ViewPointContextUI>();
        _viewPointContextUIInstance.Initialize(_viewPointBeingMoved, this);
        ToggleUnsavedChanges(false);
        ToggleViewPointContextUI(true);
    }

    public void ExitViewMoving(bool save, string message, Action<bool> success) {
        if (_viewPointBeingMoved == null) return;
        if (_unsavedChanges && !save) {
            PopUp.Instance.AreYouSurePopUp((sure) => {
                if (sure) {
                    FinishMoving(save);
                    success.Invoke(true);
                } else {
                    success.Invoke(false);
                }
            }, message);
        } else {
            FinishMoving(save);
            success.Invoke(true);
        }
    }

    /// <summary>
    /// Puts the view point back to how it was when moving started (position, rotation and name).
    /// After this, ExitViewMoving(save: false) leaves without asking.
    /// </summary>
    public void DiscardChanges() {
        if (_viewPointBeingMoved == null) return;
        RevertViewPoint();
        ToggleUnsavedChanges(false);
    }

    void FinishMoving(bool save) {
        _movingActive = false;
        if (!save) RevertViewPoint();
        _movementScript.SetTarget(null);
        ToggleViewPointContextUI(false);
    }

    void RevertViewPoint() {
        _viewPointBeingMoved.transform.position = _originalPos;
        _viewPointBeingMoved.transform.rotation = _originalRot;
        if (_originalName != null) _viewPointBeingMoved.SetName(_originalName);
    }

    public void ToggleUnsavedChanges(bool toggleOn) {
        _unsavedChanges = toggleOn;
        _viewPointContextUIInstance.ToggleSaveButton(_unsavedChanges);
    }

    public void ToggleMovementScript(bool active) {
        _movementScript.enabled = active;
    }

    public void ToggleViewPointContextUI(bool show) {
        if (_viewPointContextUIInstance == null) return;
        _viewPointContextUIInstance.gameObject.SetActive(show);
    }
}
