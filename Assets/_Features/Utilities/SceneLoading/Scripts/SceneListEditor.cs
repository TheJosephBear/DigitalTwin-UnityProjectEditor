#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneList))]
public class SceneListEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        SceneList sceneList = (SceneList)target;

        if (GUILayout.Button("Update All Scenes & Enum")) {
            AddAllProjectScenes(sceneList);
            GenerateSceneEnum(sceneList);
        }
    }

    private void AddAllProjectScenes(SceneList sceneList) {
        string[] sceneGuids = AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets" });

        sceneList.scenes.Clear();

        foreach (string guid in sceneGuids) {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            if (sceneAsset != null) {
                SceneField sceneField = new SceneField();

                sceneField.GetType().GetField("m_SceneAsset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                          .SetValue(sceneField, sceneAsset);
                sceneField.GetType().GetField("m_SceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                          .SetValue(sceneField, sceneAsset.name);

                sceneList.scenes.Add(sceneField);
            }
        }

        EditorUtility.SetDirty(sceneList);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Added {sceneList.scenes.Count} scenes to the list.");
    }

    private void GenerateSceneEnum(SceneList sceneList) {
        string enumName = "SceneType";
        string folderPath = "Assets/Enums";

        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, enumName + ".cs");

        HashSet<string> addedNames = new HashSet<string>();
        List<string> validEnumEntries = new List<string>();

        foreach (var scene in sceneList.scenes) {
            string originalName = scene.SceneName;
            string safeName = MakeSafeEnumName(originalName);

            if (string.IsNullOrEmpty(safeName)) {
                Debug.LogWarning($"Skipped scene '{originalName}' — empty or invalid enum name.");
                continue;
            }

            if (!IsValidEnumName(safeName)) {
                Debug.LogWarning($"Skipped scene '{originalName}' — contains invalid characters for enum.");
                continue;
            }

            if (addedNames.Contains(safeName)) {
                Debug.LogWarning($"Duplicate scene name detected: '{originalName}' → '{safeName}' — skipping.");
                continue;
            }

            addedNames.Add(safeName);
            validEnumEntries.Add(safeName);
        }

        if (validEnumEntries.Count == 0) {
            Debug.LogWarning("No valid scene names found — enum not generated.");
            return;
        }

        using (StreamWriter writer = new StreamWriter(filePath, false)) {
            writer.WriteLine("// Auto-generated SceneType enum");
            writer.WriteLine("public enum " + enumName);
            writer.WriteLine("{");

            for (int i = 0; i < validEnumEntries.Count; i++) {
                writer.WriteLine("    " + validEnumEntries[i] + (i < validEnumEntries.Count - 1 ? "," : ""));
            }

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();
        Debug.Log($"SceneType enum generated with {validEnumEntries.Count} entries at: {filePath}");
    }

    private bool IsValidEnumName(string name) {
        foreach (char c in name) {
            if (!char.IsLetterOrDigit(c) && c != '_') {
                return false;
            }
        }

        if (char.IsDigit(name[0])) return false;

        return true;
    }
    private string MakeSafeEnumName(string sceneName) {
        string safe = sceneName.Replace(" ", "_")
                               .Replace("-", "_")
                               .Replace(".", "_");

        safe = new string(System.Array.FindAll(safe.ToCharArray(), c =>
            char.IsLetterOrDigit(c) || c == '_'));

        if (string.IsNullOrEmpty(safe))
            return null;

        return char.IsDigit(safe[0]) ? "_" + safe : safe;
    }

}

#endif
