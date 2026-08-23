using UnityEngine;

public class SunManager: Singleton<SunManager> {
    [Header("References")]
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private SunIRL _sunIRL;
    [SerializeField] private Transform _canvasParent;

    private GameObject _uiInstance;
    private SunIRLUI _uiScript;

    protected override void Awake() {
        base.Awake();
        if (_sunIRL == null)
            _sunIRL = FindFirstObjectByType<SunIRL>();
    }

    public void ToggleUI(bool toggleOn) {
        if (_uiInstance == null) {
            InstantiateUI();
        } else {
            _uiInstance.SetActive(toggleOn);
        }
    }

    private void InstantiateUI() {
        if (_uiPrefab == null) {
            Debug.LogError("SunManager: UIPrefab reference is missing!");
            return;
        }

        _uiInstance = Instantiate(_uiPrefab, _canvasParent != null ? _canvasParent : transform);
        _uiScript = _uiInstance.GetComponent<SunIRLUI>();

        if (_uiScript != null) {
            _uiScript.Initialize(this);
        } else {
            Debug.LogError("SunManager: UIPrefab does not contain a SunIRLUI component!");
        }
    }

    #region Relay Methods to SunIRL

    public void UpdateCoordinates(float latitude, float longitude) {
        if (_sunIRL != null)
            _sunIRL.SetCoordinates(latitude, longitude);
    }

    public void UpdateDate(uint year, uint month, uint day) {
        if (_sunIRL != null) {
            _sunIRL.SetYear(year);
            _sunIRL.SetMonth(month);
            _sunIRL.SetDay(day);
        }
    }

    public void UpdateTime(uint hour, uint minute) {
        if (_sunIRL != null) {
            _sunIRL.SetHour(hour);
            _sunIRL.SetMinute(minute);
        }
    }

    #endregion
}
