using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SunIRLUI: MonoBehaviour {
    private const int TimeStepMinutes = 5;
    private const int MinutesPerDay = 24 * 60;

    // Genitive month names for "31. prosince"; hardcoded so it doesn't depend on cs-CZ culture data (WebGL)
    private static readonly string[] CzechMonthsGenitive = {
        "ledna", "února", "března", "dubna", "května", "června",
        "července", "srpna", "září", "října", "listopadu", "prosince"
    };

    [Header("Initial Values")]
    [SerializeField] private float initialLatitude = 0f;
    [SerializeField] private float initialLongitude = 0f;
    [SerializeField] private uint initialYear = 2026;
    [Range(1, 12)][SerializeField] private uint initialMonth = 1;
    [Range(1, 31)][SerializeField] private uint initialDay = 1;
    [Range(0, 23)][SerializeField] private uint initialHour = 12;
    [Range(0, 59)][SerializeField] private uint initialMinute = 0;

    [Header("Coordinates")]
    [SerializeField] private TMP_InputField latitudeInput;
    [SerializeField] private TMP_InputField longitudeInput;

    [Header("Date")]
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private UnityEngine.UI.Slider dateSlider;
    [SerializeField] private TMP_Text dateValueLabel;

    [Header("Time")]
    [SerializeField] private UnityEngine.UI.Slider timeSlider;
    [SerializeField] private TMP_Text timeValueLabel;

    [Header("Live Time")]
    [Tooltip("\"Aktuální datum a čas\": follow the real clock until the user moves a date/time control")]
    [SerializeField] private UnityEngine.UI.Toggle currentTimeToggle;

    [Header("GO References")]
    [SerializeField] private GameObject ContainerGameObject;
    [SerializeField] private GameObject ButtonGameObject;


    // What neighbouring HUD elements (e.g. the walk button) line up next to:
    // the card while the modal is open, the sun button while it is closed
    public static RectTransform CurrentAnchor { get; private set; }
    public static event Action<RectTransform, bool> AnchorChanged; // (anchor, animate)
    // The sun button itself, so neighbours can match its on-screen size
    public static RectTransform SunButton { get; private set; }

    private SunManager sunManager;
    private bool isInitializing = false;

    public void Initialize(SunManager manager, bool applyDefaults = true) {
        sunManager = manager;
        isInitializing = true;

        // 1. Setup UI fields and slider ranges
        PopulateYears();
        SetDropdownToValue(yearDropdown, initialYear.ToString());

        dateSlider.wholeNumbers = true;
        dateSlider.minValue = 1;
        UpdateDateSliderRange();
        dateSlider.SetValueWithoutNotify(DayOfYear(SelectedYear(), (int)initialMonth, (int)initialDay));

        timeSlider.wholeNumbers = true;
        timeSlider.minValue = 0;
        timeSlider.maxValue = MinutesPerDay / TimeStepMinutes - 1;
        timeSlider.SetValueWithoutNotify(TimeToSliderValue((int)initialHour, (int)initialMinute));

        RefreshDateLabel();
        RefreshTimeLabel();

        // 2. Bind Listeners
        latitudeInput.onEndEdit.AddListener(OnCoordinatesChanged);
        longitudeInput.onEndEdit.AddListener(OnCoordinatesChanged);

        yearDropdown.onValueChanged.AddListener(OnYearChanged);
        dateSlider.onValueChanged.AddListener(OnDateChanged);
        timeSlider.onValueChanged.AddListener(OnTimeChanged);

        if (currentTimeToggle != null) {
            currentTimeToggle.SetIsOnWithoutNotify(sunManager.UseCurrentTime);
            currentTimeToggle.onValueChanged.AddListener(OnCurrentTimeToggled);
        }

        isInitializing = false;

        // 3. ONLY trigger initial defaults if NOT loading saved data
        if (applyDefaults) {
            ApplyAllValuesToManager();
        }

        SetVisible(false, animate: false);
    }

    public void SetUIValues(float latitude, float longitude, int year, int month, int day, int hour, int minute) {
        bool wasInitializing = isInitializing;
        isInitializing = true; // Block UI event callbacks from re-triggering SunManager

        // 1. Set Coordinate Inputs
        if (latitudeInput != null) {
            latitudeInput.text = latitude.ToString("F4");
        }
        if (longitudeInput != null) {
            longitudeInput.text = longitude.ToString("F4");
        }

        // 2. Date and time
        SetDateTimeValues(year, month, day, hour, minute);

        isInitializing = wasInitializing; // Restore state
    }

    /// <summary>
    /// Updates only the date/time controls, so the live clock doesn't overwrite coordinates being typed.
    /// </summary>
    public void SetDateTimeValues(int year, int month, int day, int hour, int minute) {
        bool wasInitializing = isInitializing;
        isInitializing = true; // Block UI event callbacks from re-triggering SunManager

        // 1. Ensure Year options are populated, then select the year
        if (yearDropdown.options.Count == 0) PopulateYears();
        SetDropdownToValue(yearDropdown, year.ToString());

        // 2. Resize the date slider for the selected year and pick the day
        UpdateDateSliderRange();
        dateSlider.SetValueWithoutNotify(DayOfYear(SelectedYear(), month, day));
        RefreshDateLabel();

        // 3. Set the time slider (rounded down to the slider step)
        timeSlider.SetValueWithoutNotify(TimeToSliderValue(hour, minute));
        RefreshTimeLabel();

        isInitializing = wasInitializing; // Restore state
    }

    public void SetCurrentTimeToggle(bool isOn) {
        if (currentTimeToggle != null) {
            currentTimeToggle.SetIsOnWithoutNotify(isOn);
        }
    }

    public void OnShowUI() {
        SetVisible(true, animate: true);
    }

    public void OnHideUI() {
        SetVisible(false, animate: true);
    }

    private void SetVisible(bool visible, bool animate) {
        ButtonGameObject.SetActive(!visible);
        ContainerGameObject.SetActive(visible);

        SunButton = (RectTransform)ButtonGameObject.transform;
        CurrentAnchor = (RectTransform)(visible ? ContainerGameObject : ButtonGameObject).transform;
        AnchorChanged?.Invoke(CurrentAnchor, animate);
    }

    private void OnDestroy() {
        if (CurrentAnchor != null && CurrentAnchor.IsChildOf(transform)) {
            CurrentAnchor = null;
        }
        if (SunButton != null && SunButton.IsChildOf(transform)) {
            SunButton = null;
        }
    }

    private void ApplyAllValuesToManager() {
        if (sunManager == null) return;

        sunManager.UpdateCoordinates(initialLatitude, initialLongitude);
        sunManager.UpdateDate(initialYear, initialMonth, initialDay);
        sunManager.UpdateTime(initialHour, initialMinute);
    }

    private void PopulateYears() {
        yearDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentYear = DateTime.Now.Year;

        for (int y = currentYear - 10; y <= currentYear + 10; y++) {
            options.Add(y.ToString());
        }

        yearDropdown.AddOptions(options);
    }

    private int SelectedYear() {
        return int.Parse(yearDropdown.options[yearDropdown.value].text);
    }

    private void UpdateDateSliderRange() {
        int daysInYear = DateTime.IsLeapYear(SelectedYear()) ? 366 : 365;
        dateSlider.maxValue = daysInYear;
        dateSlider.SetValueWithoutNotify(Mathf.Clamp(dateSlider.value, 1, daysInYear));
    }

    private DateTime DateFromSlider() {
        return new DateTime(SelectedYear(), 1, 1).AddDays((int)dateSlider.value - 1);
    }

    private static int DayOfYear(int year, int month, int day) {
        month = Mathf.Clamp(month, 1, 12);
        day = Mathf.Clamp(day, 1, DateTime.DaysInMonth(year, month));
        return new DateTime(year, month, day).DayOfYear;
    }

    private static int TimeToSliderValue(int hour, int minute) {
        int totalMinutes = Mathf.Clamp(hour, 0, 23) * 60 + Mathf.Clamp(minute, 0, 59);
        return totalMinutes / TimeStepMinutes;
    }

    private int SliderTotalMinutes() {
        return (int)timeSlider.value * TimeStepMinutes;
    }

    private void RefreshDateLabel() {
        if (dateValueLabel != null) {
            DateTime date = DateFromSlider();
            dateValueLabel.text = $"{date.Day}. {CzechMonthsGenitive[date.Month - 1]}";
        }
    }

    private void RefreshTimeLabel() {
        if (timeValueLabel != null) {
            int totalMinutes = SliderTotalMinutes();
            timeValueLabel.text = $"{totalMinutes / 60:D2}:{totalMinutes % 60:D2}";
        }
    }

    private void SetDropdownToValue(TMP_Dropdown dropdown, string valueStr) {
        int index = dropdown.options.FindIndex(option => option.text == valueStr);
        if (index != -1) {
            dropdown.SetValueWithoutNotify(index);
            dropdown.RefreshShownValue();
        }
    }

    #region Event Callbacks

    private void OnCoordinatesChanged(string _) {
        if (isInitializing) return;

        float.TryParse(latitudeInput.text, out float lat);
        float.TryParse(longitudeInput.text, out float lon);

        sunManager.UpdateCoordinates(lat, lon);
    }

    private void OnCurrentTimeToggled(bool isOn) {
        if (isInitializing) return;

        sunManager.SetUseCurrentTime(isOn);
    }

    // Any manual date/time change "locks" the time: the live clock stops and the value stays fixed
    private void LockToManualTime() {
        if (!sunManager.UseCurrentTime) return;

        SetCurrentTimeToggle(false);
        sunManager.SetUseCurrentTime(false);
    }

    private void OnYearChanged(int _) {
        if (isInitializing) return;

        LockToManualTime();
        UpdateDateSliderRange();
        OnDateChanged(dateSlider.value);
    }

    private void OnDateChanged(float _) {
        RefreshDateLabel();
        if (isInitializing) return;

        LockToManualTime();
        DateTime date = DateFromSlider();
        sunManager.UpdateDate((uint)date.Year, (uint)date.Month, (uint)date.Day);
    }

    private void OnTimeChanged(float _) {
        RefreshTimeLabel();
        if (isInitializing) return;

        LockToManualTime();
        int totalMinutes = SliderTotalMinutes();
        sunManager.UpdateTime((uint)(totalMinutes / 60), (uint)(totalMinutes % 60));
    }

    #endregion
}
