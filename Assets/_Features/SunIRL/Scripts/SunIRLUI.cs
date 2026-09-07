using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SunIRLUI: MonoBehaviour {
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

    [Header("Date Dropdowns")]
    [SerializeField] private TMP_Dropdown yearDropdown;
    [SerializeField] private TMP_Dropdown monthDropdown;
    [SerializeField] private TMP_Dropdown dayDropdown;

    [Header("Time Input")]
    [SerializeField] private TMP_InputField hourInput;
    [SerializeField] private TMP_InputField minuteInput;

    [Header("GO References")]
    [SerializeField] private GameObject ContainerGameObject;
    [SerializeField] private GameObject ButtonGameObject;


    private SunManager sunManager;
    private bool isInitializing = false;

    public void Initialize(SunManager manager, bool applyDefaults = true) {
        sunManager = manager;
        isInitializing = true;

        // 1. Setup UI fields and dropdown options
        PopulateYears();
        PopulateMonths();

        // Build days according to initial year/month, then select day
        UpdateDays();
        dayDropdown.value = (int)Mathf.Clamp(initialDay - 1, 0, dayDropdown.options.Count - 1);
        dayDropdown.RefreshShownValue();

        // 2. Bind Listeners
        latitudeInput.onEndEdit.AddListener(OnCoordinatesChanged);
        longitudeInput.onEndEdit.AddListener(OnCoordinatesChanged);

        yearDropdown.onValueChanged.AddListener(OnYearOrMonthChanged);
        monthDropdown.onValueChanged.AddListener(OnYearOrMonthChanged);
        dayDropdown.onValueChanged.AddListener(OnDateChanged);

        hourInput.onEndEdit.AddListener(OnTimeChanged);
        minuteInput.onEndEdit.AddListener(OnTimeChanged);

        isInitializing = false;

        // 3. ONLY trigger initial defaults if NOT loading saved data
        if (applyDefaults) {
            ApplyAllValuesToManager();
        }

        OnHideUI();
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

        // 2. Ensure Year and Month options are populated
        if (yearDropdown.options.Count == 0) PopulateYears();
        if (monthDropdown.options.Count == 0) PopulateMonths();

        // 3. Set Year Dropdown
        SetDropdownToValue(yearDropdown, year.ToString());

        // 4. Set Month Dropdown (1-indexed input to 0-indexed dropdown index)
        int monthIndex = Mathf.Clamp(month - 1, 0, 11);
        monthDropdown.value = monthIndex;
        monthDropdown.RefreshShownValue();

        // 5. Repopulate Days according to Year/Month and pick Day
        UpdateDays();
        int dayIndex = Mathf.Clamp(day - 1, 0, dayDropdown.options.Count - 1);
        dayDropdown.value = dayIndex;
        dayDropdown.RefreshShownValue();

        // 6. Set Time Inputs (Formatted with two digits)
        if (hourInput != null) {
            hourInput.text = Mathf.Clamp(hour, 0, 23).ToString("D2");
        }
        if (minuteInput != null) {
            minuteInput.text = Mathf.Clamp(minute, 0, 59).ToString("D2");
        }

        isInitializing = wasInitializing; // Restore state
    }

    public void OnShowUI() {
        ButtonGameObject.SetActive(false);
        ContainerGameObject.SetActive(true);
    }

    public void OnHideUI() {
        ButtonGameObject.SetActive(true);
        ContainerGameObject.SetActive(false);
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

    private void PopulateMonths() {
        monthDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int m = 1; m <= 12; m++) {
            options.Add(new DateTime(2026, m, 1).ToString("MMMM"));
        }

        monthDropdown.AddOptions(options);
    }

    private void UpdateDays() {
        if (yearDropdown.options.Count == 0 || monthDropdown.options.Count == 0) return;

        int selectedYear = int.Parse(yearDropdown.options[yearDropdown.value].text);
        int selectedMonth = monthDropdown.value + 1;
        int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);

        int previousDayValue = dayDropdown.value;

        dayDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int d = 1; d <= daysInMonth; d++) {
            options.Add(d.ToString("D2"));
        }

        dayDropdown.AddOptions(options);
        dayDropdown.value = Mathf.Clamp(previousDayValue, 0, daysInMonth - 1);
        dayDropdown.RefreshShownValue();
    }

    private void SetDropdownToValue(TMP_Dropdown dropdown, string valueStr) {
        int index = dropdown.options.FindIndex(option => option.text == valueStr);
        if (index != -1) {
            dropdown.value = index;
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

    private void OnYearOrMonthChanged(int _) {
        if (isInitializing) return;

        UpdateDays();
        OnDateChanged(dayDropdown.value);
    }

    private void OnDateChanged(int _) {
        if (isInitializing) return;

        uint year = uint.Parse(yearDropdown.options[yearDropdown.value].text);
        uint month = (uint)(monthDropdown.value + 1);
        uint day = (uint)(dayDropdown.value + 1);

        sunManager.UpdateDate(year, month, day);
    }

    private void OnTimeChanged(string _) {
        if (isInitializing) return;

        uint.TryParse(hourInput.text, out uint hour);
        uint.TryParse(minuteInput.text, out uint minute);

        sunManager.UpdateTime(hour, minute);
    }

    #endregion
}
