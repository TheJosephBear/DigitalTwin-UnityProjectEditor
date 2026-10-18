using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Places a new pin or moves an existing one: the pin follows the cursor until a click confirms or Escape cancels.
/// </summary>
public class PinPlacingManager : MonoBehaviour {

    public GameObject PlacingLabelPrefab;
    [SerializeField] private LayerMask _placementRaycastMask = ~0; // Default: Everything

    const string PlacingLabelText = "Klikněte na místo, kam chcete umístit pin.";

    GameObject _placingLabelInstance;
    PinObject _pinBeingPlaced;
    Vector3 _originalPos;
    bool _isNew = false;
    bool _placingActive = false;
    int _finishedFrame = -1;
    AppState _previousState;

    // Also true in the frame placing ended, so the confirming click doesn't select the pin under the cursor
    public bool IsPlacing => _placingActive || Time.frameCount == _finishedFrame;

    private void Update() {
        if (!_placingActive) return;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            FinishPlacing(confirmed: false);
            return;
        }

        HandlePlacingInput();
    }

    private void HandlePlacingInput() {
        Camera cam = Camera.main;
        if (cam == null) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // Raycast to track cursor in real time
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _placementRaycastMask)) {
            _pinBeingPlaced.transform.position = hit.point;
            // Tilted so it touches the target
            if (Mathf.Abs(hit.normal.y) < 0.3f) {
                Quaternion wallRotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
                _pinBeingPlaced.transform.rotation = wallRotation * Quaternion.Euler(-15f, 0f, 0f);
            } else {
                _pinBeingPlaced.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            // Left Click to set position
            if (Input.GetMouseButtonDown(0)) {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
                    return;
                }

                FinishPlacing(confirmed: true);
            }
        }
    }

    /// <summary>
    /// Starts placing the given pin, or a new pin when null.
    /// </summary>
    public void StartPlacing(PinObject pin) {
        if (_placingActive) return;

        _isNew = pin == null;
        if (_isNew) {
            pin = PinManager.Instance.CreatePinObject(GetSpawnPosition());
            if (pin == null) return;
        }

        _pinBeingPlaced = pin;
        _originalPos = pin.transform.position;
        _pinBeingPlaced.SetPlacingMode(true);

        ToggleLabel(true);
        TogglePinUIs(false);
        _placingActive = true;

        // The state hides the rest of the editor UI; without a main manager only the pin UIs are hidden
        if (MainManagerBase.Instance != null) {
            _previousState = MainManagerBase.Instance.ActiveState;
            MainManagerBase.Instance.ChangeState(AppState.PinPlacing);
        }
    }

    private void FinishPlacing(bool confirmed) {
        PinObject pin = _pinBeingPlaced;
        bool isNew = _isNew;
        EndPlacing(confirmed);

        if (MainManagerBase.Instance != null) MainManagerBase.Instance.ChangeState(_previousState);
        PinManager.Instance.TogglePinUI(true);
        PinManager.Instance.OnPlacingFinished(pin, isNew, confirmed);
    }

    /// <summary>
    /// Stops placing without changing the app state, e.g. when leaving the editor.
    /// </summary>
    public void CancelPlacing() {
        if (!_placingActive) return;

        PinObject pin = _pinBeingPlaced;
        bool isNew = _isNew;
        EndPlacing(confirmed: false);
        if (isNew) Destroy(pin.gameObject);
    }

    private void EndPlacing(bool confirmed) {
        _placingActive = false;
        _finishedFrame = Time.frameCount;
        if (!confirmed) _pinBeingPlaced.transform.position = _originalPos;
        _pinBeingPlaced.SetPlacingMode(false);
        _pinBeingPlaced = null;
        ToggleLabel(false);
    }

    private Vector3 GetSpawnPosition() {
        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;
        return cam.transform.position + cam.transform.forward * 2f;
    }

    private void TogglePinUIs(bool show) {
        PinManager.Instance.TogglePinUI(show);
        PinManager.Instance.ToggleContextUI(show);
    }

    private void ToggleLabel(bool show) {
        if (_placingLabelInstance == null) {
            if (!show || PlacingLabelPrefab == null) return;
            _placingLabelInstance = SceneLoadingManager.Instance != null ?
                SceneLoadingManager.Instance.InstantiateObjectInScene(PlacingLabelPrefab)
                : Instantiate(PlacingLabelPrefab);
            TextMeshProUGUI label = _placingLabelInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = PlacingLabelText;
        }

        _placingLabelInstance.SetActive(show);
    }
}
