using System;
using System.IO;

/// <summary>
/// Where the test models are on this machine. Each folder is taken from, in this order:
///   1. an environment variable (for command line runs): DT_TEST_MODELS_ROOT, DT_TEST_AKAGI_ROOT
///   2. Tools > Model Upload Tests > Set Model Folders (saved for this computer, not in git)
///   3. the default below
/// </summary>
public static class TestModelFolders {

    public class Folder {
        public readonly string Label;
        public readonly string EnvVar;
        public readonly string PrefsKey;
        public readonly string Default;
        public readonly string Contents; // what the folder should hold, shown when picking it

        public Folder(string label, string envVar, string prefsKey, string defaultPath, string contents) {
            Label = label; EnvVar = envVar; PrefsKey = prefsKey; Default = defaultPath; Contents = contents;
        }

        public string Path {
            get {
                string env = Environment.GetEnvironmentVariable(EnvVar);
                if (!string.IsNullOrWhiteSpace(env)) return env.Trim();
#if UNITY_EDITOR
                string saved = UnityEditor.EditorPrefs.GetString(PrefsKey, "");
                if (!string.IsNullOrWhiteSpace(saved)) return saved;
#endif
                return Default;
            }
        }

        public string Source {
            get {
                if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvVar))) return $"environment variable {EnvVar}";
#if UNITY_EDITOR
                if (!string.IsNullOrWhiteSpace(UnityEditor.EditorPrefs.GetString(PrefsKey, ""))) return "set on this computer";
#endif
                return "default";
            }
        }

        public bool Exists => Directory.Exists(Path);
    }

    public static readonly Folder DTModels = new Folder(
        "DT modely", "DT_TEST_MODELS_ROOT", "ModelUploadTests.DTModelsRoot", @"F:\Programming\DT modely",
        "the 'DT modely' folder (Carpincho, MFC_testovani_obj, from server, Litvínov, ...)");

    public static readonly Folder Akagi = new Folder(
        "Akagi", "DT_TEST_AKAGI_ROOT", "ModelUploadTests.AkagiRoot", @"D:\Unity stuff\akagi mountain pass",
        "the 'akagi mountain pass' folder (texture_png, fixed_decimal_dots, akagi mountain pass.fbx)");

    public static readonly Folder[] All = { DTModels, Akagi };

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Model Upload Tests/Set Model Folders...")]
    static void SetFolders() {
        foreach (Folder folder in All) {
            int choice = UnityEditor.EditorUtility.DisplayDialogComplex("Model Upload Tests",
                $"{folder.Label} models: {folder.Path}\n({folder.Source}{(folder.Exists ? "" : ", folder not found")})\n\n" +
                $"Pick {folder.Contents}.",
                "Pick Folder", "Keep", "Reset to Default");

            if (choice == 0) {
                string start = folder.Exists ? folder.Path : "";
                string picked = UnityEditor.EditorUtility.OpenFolderPanel($"{folder.Label} models", start, "");
                if (!string.IsNullOrEmpty(picked)) UnityEditor.EditorPrefs.SetString(folder.PrefsKey, picked.Replace('/', '\\'));
            } else if (choice == 2) {
                UnityEditor.EditorPrefs.DeleteKey(folder.PrefsKey);
            }
        }
        ShowFolders();
    }

    [UnityEditor.MenuItem("Tools/Model Upload Tests/Show Model Folders")]
    static void ShowFolders() {
        string text = "";
        foreach (Folder folder in All)
            text += $"{folder.Label}: {folder.Path}\n({folder.Source}, {(folder.Exists ? "found" : "NOT FOUND")})\n\n";
        text += "Models whose files aren't found are skipped, not failed.";
        UnityEditor.EditorUtility.DisplayDialog("Model Upload Tests", text, "OK");
    }
#endif
}
