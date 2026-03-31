using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
/// <summary>
/// Tool that plays an Audio clip, when you get an error message.
/// </summary>
[InitializeOnLoad]
public static class SpicyErrorNotifications {
    private const string MENU_PATH = "Tools/Error Sound/Enabled";

    private static AudioClip errorClip;
    private static AudioClip warningClip;

    private static double lastPlayTime;
    private const double cooldown = 0.3;

    private static bool Enabled {
        get => EditorPrefs.GetBool(MENU_PATH, true);
        set => EditorPrefs.SetBool(MENU_PATH, value);
    }

    static SpicyErrorNotifications() {
        // Load audio clips from Resources
        errorClip = Resources.Load<AudioClip>("spicy_error_sound");
        warningClip = Resources.Load<AudioClip>("spicy_warning_sound");

        Application.logMessageReceived += OnLogMessage;

        // Sync menu checkmark
        Menu.SetChecked(MENU_PATH, Enabled);
    }

    private static void OnLogMessage(string condition, string stackTrace, LogType type) {
        if (!Enabled) return;

        // Cooldown to prevent spam
        if (EditorApplication.timeSinceStartup - lastPlayTime < cooldown)
            return;

        switch (type) {
            case LogType.Error:
            case LogType.Exception:
                PlayClip(errorClip);
                break;

            case LogType.Warning:
                PlayClip(warningClip);
                break;
        }

        lastPlayTime = EditorApplication.timeSinceStartup;
    }

    private static void PlayClip(AudioClip clip) {
        if (clip == null) return;

        var editorAssembly = typeof(AudioImporter).Assembly;
        var audioUtilClass = editorAssembly.GetType("UnityEditor.AudioUtil");

        var method = audioUtilClass.GetMethod(
            "PlayPreviewClip",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
            null,
            new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );

        method?.Invoke(null, new object[] { clip, 0, false });
    }

    // Toggle Menu Item
    [MenuItem(MENU_PATH)]
    private static void Toggle() {
        Enabled = !Enabled;
        Menu.SetChecked(MENU_PATH, Enabled);
    }
}

#endif