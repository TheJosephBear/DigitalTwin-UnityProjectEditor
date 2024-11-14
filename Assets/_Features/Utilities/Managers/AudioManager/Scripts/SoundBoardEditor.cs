using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR

[CustomEditor(typeof(SoundBoard))]
public class SoundBoardEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        SoundBoard soundBoard = (SoundBoard)target;

        if (GUILayout.Button("Add Sounds from Folder")) {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Sounds", Application.dataPath, "");
            if (!string.IsNullOrEmpty(folderPath)) {
                AddSoundsFromFolder(soundBoard, folderPath);
            }
        }

        if (GUILayout.Button("Generate Sound Enum")) {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Sounds", Application.dataPath, "");
            if (!string.IsNullOrEmpty(folderPath)) {
                GenerateSoundEnum(folderPath);
            }
        }
    }

    private void AddSoundsFromFolder(SoundBoard soundBoard, string folderPath) {
        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
        string[] soundPaths = Directory.GetFiles(relativePath, "*.asset");

        soundBoard.sounds.Clear();

        foreach (string soundPath in soundPaths) {
            Sound sound = AssetDatabase.LoadAssetAtPath<Sound>(soundPath);
            if (sound != null) {
                soundBoard.sounds.Add(sound);
            }
        }

        EditorUtility.SetDirty(soundBoard);
        AssetDatabase.SaveAssets();
    }

    private void GenerateSoundEnum(string folderPath) {
        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
        string[] soundPaths = Directory.GetFiles(relativePath, "*.asset");

        List<string> soundNames = new List<string>();

        foreach (string soundPath in soundPaths) {
            Sound sound = AssetDatabase.LoadAssetAtPath<Sound>(soundPath);
            if (sound != null) {
                soundNames.Add(sound.soundName);
            }
        }

        string enumName = "SoundType";
        string filePath = "Assets/Enums/SoundType.cs";

        using (StreamWriter writer = new StreamWriter(filePath, false)) {
            writer.WriteLine("public enum " + enumName);
            writer.WriteLine("{");

            for (int i = 0; i < soundNames.Count; i++) {
                string name = soundNames[i].Replace(" ", ""); ;
                writer.WriteLine("    " + name + (i < soundNames.Count - 1 ? "," : ""));
            }

            writer.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }

}

#endif
