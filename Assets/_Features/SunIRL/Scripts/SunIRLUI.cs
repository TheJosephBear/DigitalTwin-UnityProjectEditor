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

    private SunManager sunManager;
    private bool isInitializing = false;

    public void Initialize(SunManager manager) {
        sunManager = manager;
        isInitializing = true;

        // 1. Setup UI fields and dropdown options
        PopulateYears();
        PopulateMonths();

        // Apply Initial Values to UI
        latitudeInput.text = initialLatitude.ToString();
        longitudeInput.text = initialLongitude.ToString();
        hourInput.text = initialHour.ToString("D2");
        minuteInput.text = initialMinute.ToString("D2");

        SetDropdownToValue(yearDropdown, initialYear.ToString());
        monthDropdown.value = (int)Mathf.Clamp(initialMonth - 1, 0, 11);

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

        // 3. Trigger updates on manager with initial values
        ApplyAllValuesToManager();
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
