using System;
using UnityEngine;

// The one place icon colors are set. All icon PNGs are white, so a tint shows them in exactly this color.
// uGUI icons follow it through ThemedIcon; the survey (UI Toolkit) through IconTheme.uss, which is
// rewritten from this asset whenever it changes in the editor.
// The asset lives in Resources/IconTheme.asset.
[CreateAssetMenu(fileName = "IconTheme", menuName = "UI/Icon Theme")]
public class IconTheme : ScriptableObject {
    public enum Role {
        Icon,   // On light backgrounds: the default
        OnDark  // On dark or colored backgrounds (map buttons, photos, filled buttons)
    }

    private const string ResourcePath = "IconTheme";

    [Tooltip("Color of every icon on a light background")]
    [SerializeField] private Color iconColor = new Color32(0x6A, 0x58, 0xA7, 0xFF);

    [Tooltip("Color of icons on dark or colored backgrounds")]
    [SerializeField] private Color iconOnDarkColor = Color.white;

    public Color IconColor => iconColor;
    public Color IconOnDarkColor => iconOnDarkColor;

    public Color ColorFor(Role role) => role == Role.OnDark ? iconOnDarkColor : iconColor;

    // Raised when the colors change in the editor, so open scenes update right away
    public static event Action Changed;

    private static IconTheme _current;
    public static IconTheme Current {
        get {
            if (_current == null) _current = Resources.Load<IconTheme>(ResourcePath);
            return _current;
        }
    }

#if UNITY_EDITOR
    public const string UssPath = "Assets/_Features/Utilities/UI/IconTheme/IconTheme.uss";

    private void OnValidate() {
        Changed?.Invoke();
        // Writing and importing an asset isn't allowed during OnValidate
        UnityEditor.EditorApplication.delayCall += WriteUss;
    }

    // The survey stylesheets @import this file and tint their icons with these variables
    public void WriteUss() {
        if (this == null) return;
        string uss =
            "/* Generated from Resources/IconTheme.asset (IconTheme.cs). Change the colors there, not here. */\n" +
            ":root {\n" +
            $"    --icon-color: {ToUss(iconColor)};\n" +
            $"    --icon-on-dark-color: {ToUss(iconOnDarkColor)};\n" +
            "}\n";
        string fullPath = System.IO.Path.GetFullPath(UssPath);
        if (System.IO.File.Exists(fullPath) && System.IO.File.ReadAllText(fullPath) == uss) return;
        System.IO.File.WriteAllText(fullPath, uss);
        UnityEditor.AssetDatabase.ImportAsset(UssPath);
    }

    private static string ToUss(Color c) {
        Color32 c32 = c;
        return c32.a == 255
            ? $"rgb({c32.r}, {c32.g}, {c32.b})"
            : $"rgba({c32.r}, {c32.g}, {c32.b}, {(c32.a / 255f).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)})";
    }
#endif
}
