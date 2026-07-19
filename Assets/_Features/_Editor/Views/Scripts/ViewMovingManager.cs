using System;
using UnityEngine;

public class ViewMovingManager : MonoBehaviour {

    public GameObject ViewPointContextUIPrefab;

    CustomMovement _movementScript;
    ViewPoint _viewPointBeingMoved;
    ViewPointContextUI _viewPointContextUIInstance;
    Vector3 _originalPos;
    Quaternion _originalRot;
    bool _unsavedChanges = false;
    bool _movingActive = false;

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
        // Start controlling it
        _movementScript.SetTarget(_viewPointBeingMoved.gameObject);
        // Show context UI
        if (_viewPointContextUIInstance == null)
            _viewPointContextUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(ViewPointContextUIPrefab).GetComponent<ViewPointContextUI>();
        _viewPointContextUIInstance.Initialize(_viewPointBeingMoved);
        ToggleUnsavedChanges(false);
        ToggleViewPointContextUI(true);
    }

    public void ExitViewMoving(bool save, Action<bool> success) {
        if (_viewPointBeingMoved == null) return;
        if (_unsavedChanges) {
            PopUp.Instance.AreYouSurePopUp((sure) => {
                if (sure) {
                    _movingActive = false;
                    if (!save && _originalPos != null && _originalRot != null) {
                        _viewPointBeingMoved.transform.position = _originalPos;
                        _viewPointBeingMoved.transform.rotation = _originalRot;
                    }

                    _movementScript.SetTarget(null);
                    ToggleViewPointContextUI(false);
                    success.Invoke(true);
                } else {
                    success.Invoke(false);
                }
            }, "Neuložené zmìny, chcete odejít?");
        } else {
            _movingActive = false;
            if (!save && _originalPos != null && _originalRot != null) {
                _viewPointBeingMoved.transform.position = _originalPos;
                _viewPointBeingMoved.transform.rotation = _originalRot;
            }

            _movementScript.SetTarget(null);
            ToggleViewPointContextUI(false);
            success.Invoke(true);
        }
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
