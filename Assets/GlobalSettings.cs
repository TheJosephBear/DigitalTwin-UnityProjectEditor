using System;
using UnityEngine;

public class GlobalSettings: Singleton<GlobalSettings> {

    private const string SAVE_KEY = "GlobalSettingsData";
    public static event Action<SettingsData> OnSettingsLoadedOrChanged;

    [Serializable]
    public class SettingsData {
        public bool IsCameraCollisionOn = true;
        public int GraphicsQuality = 2;
    }

    public SettingsData Data { get; private set; } = new SettingsData();
    public bool IsLoaded { get; private set; }

    protected override void Awake() {
        base.Awake();
        Load();
    }

    /// <summary>
    /// Modifies any setting and auto-saves in a single readable line.
    /// Usage: GlobalSettings.Instance.Set(d => d.MasterVolume = 0.8f);
    /// </summary>
    public void Set(Action<SettingsData> changeAction) {
        changeAction(Data);
        Save();
    }

    public void Save() {
        string json = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void Load() {
        if (PlayerPrefs.HasKey(SAVE_KEY)) {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            JsonUtility.FromJsonOverwrite(json, Data);
        }

        IsLoaded = true;
    }
}
