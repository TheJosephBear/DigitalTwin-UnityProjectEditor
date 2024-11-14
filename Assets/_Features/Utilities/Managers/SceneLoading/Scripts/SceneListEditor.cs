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

        if (GUILayout.Button("Add Scenes from Folder")) {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Scenes", Application.dataPath, "");
            if (!string.IsNullOrEmpty(folderPath)) {
                AddScenesFromFolder(sceneList, folderPath);
            }
        }

        if (GUILayout.Button("Generate Scenes Enum")) {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Scenes", Application.dataPath, "");
            if (!string.IsNullOrEmpty(folderPath)) {
                GenerateSceneEnum(folderPath);
            }
        }
    }

    private void AddScenesFromFolder(SceneList sceneList, string folderPath) {
        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length).Replace("\\", "/");
        string[] scenePaths = Directory.GetFiles(relativePath, "*.unity");

        sceneList.scenes.Clear();

        foreach (string scenePath in scenePaths) {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset != null) {
                SceneField sceneField = new SceneField();
                sceneField.GetType().GetField("m_SceneAsset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sceneField, sceneAsset);
                sceneField.GetType().GetField("m_SceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sceneField, sceneAsset.name);

                sceneList.scenes.Add(sceneField);
            }
        }

        EditorUtility.SetDirty(sceneList);
        AssetDatabase.SaveAssets();
    }

    private void GenerateSceneEnum(string folderPath) {
        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length).Replace("\\", "/");
        string[] scenePaths = Directory.GetFiles(relativePath, "*.unity");

        List<string> sceneNames = new List<string>();

        foreach (string scenePath in scenePaths) {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset != null) {
                sceneNames.Add(sceneAsset.name);
            }
        }

        string enumName = "SceneType";
        string filePath = "Assets/Enums/SceneType.cs";

        using (StreamWriter writer = new StreamWriter(filePath, false)) {
            writer.WriteLine("public enum " + enumName);
            writer.WriteLine("{");

            for (int i = 0; i < sceneNames.Count; i++) {
                string name = sceneNames[i].Replace(" ", "");
                writer.WriteLine("    " + name + (i < sceneNames.Count - 1 ? "," : ""));
            }

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }
}

#endif