using System;
using System.Collections;
using UnityEngine;

public class SunManager: Singleton<SunManager> {
    // How often the sun follows the real clock while "Aktuální datum a čas" is on
    private const float LiveTimeRefreshSeconds = 15f;

    [Header("References")]
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private SunIRL _sunIRL;
    [SerializeField] private Transform _canvasParent;

    private GameObject _uiInstance;
    private SunIRLUI _uiScript;

    private bool _useCurrentTime;
    private Coroutine _liveTimeRoutine;

    protected override void Awake() {
        base.Awake();
        if (_sunIRL == null)
            _sunIRL = FindFirstObjectByType<SunIRL>();
    }

    private void OnDisable() {
        StopLiveTime();
    }

    public void ToggleUI(bool toggleOn) {
        if (_uiInstance == null) {
            InstantiateUI();
        } else {
            _uiInstance.SetActive(toggleOn);
        }
    }

    private void InstantiateUI(bool applyDefaults = true) {
        if (_uiPrefab == null) {
            Debug.LogError("SunManager: UIPrefab reference is missing!");
            return;
        }

        if (_uiInstance == null) {
            _uiInstance = Instantiate(_uiPrefab, _canvasParent != null ? _canvasParent : transform);
            _uiScript = _uiInstance.GetComponent<SunIRLUI>();
        }

        if (_uiScript != null) {
            _uiScript.Initialize(this, applyDefaults);
        } else {
            Debug.LogError("SunManager: UIPrefab does not contain a SunIRLUI component!");
        }
    }

    #region Relay Methods to SunIRL

    // Latitude, longitude the sun is calculated for
    public Vector2 Coordinates => _sunIRL != null ? new Vector2(_sunIRL.Latitude, _sunIRL.Longitude) : Vector2.zero;

    public void UpdateCoordinates(float latitude, float longitude) {
        if (_sunIRL != null)
            _sunIRL.SetCoordinates(latitude, longitude);
    }

    /// <summary>
    /// The project was placed on the map: the sun and the coordinate fields move to the new location.
    /// </summary>
    public void SetGeoCoordinates(float latitude, float longitude) {
        UpdateCoordinates(latitude, longitude);
        _uiScript?.SetCoordinateValues(latitude, longitude);
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

    #region Current Time

    public bool UseCurrentTime => _useCurrentTime;

    /// <summary>
    /// Turns the "Aktuální datum a čas" mode on/off. While on, the sun follows the real clock;
    /// turning it off keeps the last applied time as a fixed value.
    /// </summary>
    public void SetUseCurrentTime(bool useCurrentTime) {
        _useCurrentTime = useCurrentTime;

        if (useCurrentTime) {
            ApplyCurrentTime();
            if (_liveTimeRoutine == null && isActiveAndEnabled)
                _liveTimeRoutine = StartCoroutine(LiveTimeCoroutine());
        } else {
            StopLiveTime();
        }
    }

    private IEnumerator LiveTimeCoroutine() {
        var wait = new WaitForSecondsRealtime(LiveTimeRefreshSeconds);
        while (_useCurrentTime) {
            yield return wait;
            ApplyCurrentTime();
        }
        _liveTimeRoutine = null;
    }

    private void StopLiveTime() {
        if (_liveTimeRoutine != null) {
            StopCoroutine(_liveTimeRoutine);
            _liveTimeRoutine = null;
        }
    }

    private void ApplyCurrentTime() {
        if (_sunIRL == null) return;

        DateTime now = _sunIRL.GetCurrentLocalTime();
        _sunIRL.SetDate(now);
        _uiScript?.SetDateTimeValues(now.Year, now.Month, now.Day, now.Hour, now.Minute);
    }

    #endregion

    public SerializableSun Serialize() {
        SerializableSun serializedSun = _sunIRL.Serialize();
        serializedSun.useCurrentTime = _useCurrentTime;
        return serializedSun;
    }

    public void Deserialize(SerializableSun serializedSun, Vector2 geoCoordinates) {
        // A project that never saved its sun (freshly created, or saved before the sun existed) comes back
        // as null or zeroed by JsonUtility; start it at the moment it was opened, i.e. its creation time.
        bool isFreshProject = serializedSun == null || serializedSun.year == 0;

        if (isFreshProject) {
            _sunIRL.SetDate(_sunIRL.GetCurrentLocalTime());
            // Also 0, 0 for a project not placed on the map, so it doesn't keep the previous project's location
            _sunIRL.SetCoordinates(geoCoordinates.x, geoCoordinates.y);
        } else {
            _sunIRL.Deserialize(serializedSun, geoCoordinates);
        }

        InstantiateUI(applyDefaults: false);
        ToggleUI(false);

        // Mirror the sun's actual (validated) state rather than the raw saved values
        DateTime sunTime = _sunIRL.GetDateTime();
        _uiScript?.SetUIValues(
            geoCoordinates.x,
            geoCoordinates.y,
            sunTime.Year,
            sunTime.Month,
            sunTime.Day,
            sunTime.Hour,
            sunTime.Minute
        );

        bool useCurrentTime = !isFreshProject && serializedSun.useCurrentTime;
        _uiScript?.SetCurrentTimeToggle(useCurrentTime);
        SetUseCurrentTime(useCurrentTime);
    }
}
