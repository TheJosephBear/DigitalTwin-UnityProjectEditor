using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
public class UITypeEnumGenerator {

    [MenuItem("Tools/Generate UIType Enum")]
    public static void GenerateEnum() {
        // Find all UIBehaviour components in the scene
        var uiBehaviours = UnityEngine.Object.FindObjectsByType<UIBehaviour>(FindObjectsSortMode.None);
        var enumNames = new List<string>();

        foreach (var uiObject in uiBehaviours) {
            if (uiObject != null && uiObject.gameObject != null) {
                string validName = MakeValidEnumName(uiObject.gameObject.name);

                if (!enumNames.Contains(validName)) {
                    enumNames.Add(validName);
                }
            }
        }

        // Dynamically find the folder where this script is located in the project
        string scriptDirectory = GetCurrentScriptDirectory();
        string targetDirectory = Path.Combine(scriptDirectory, "Enums");

        if (!Directory.Exists(targetDirectory)) {
            Directory.CreateDirectory(targetDirectory);
        }

        string filePath = Path.Combine(targetDirectory, "UIType.cs");

        using (StreamWriter writer = new StreamWriter(filePath, false)) {
            writer.WriteLine("public enum UIType");
            writer.WriteLine("{");

            for (int i = 0; i < enumNames.Count; i++) {
                writer.WriteLine($"    {enumNames[i]} = {i},");
            }

            writer.WriteLine("}");
        }

        // Refresh the editor to recognize the new script
        AssetDatabase.Refresh();
        Debug.Log($"Enum 'UIType' generated successfully at {filePath}");
    }

    private static string GetCurrentScriptDirectory() {
        // Search for this script file by name to get its relative asset path
        string[] guids = AssetDatabase.FindAssets("UITypeEnumGenerator");
        if (guids.Length > 0) {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return Path.GetDirectoryName(path);
        }
        // Fallback to Assets if something goes wrong
        return "Assets";
    }

    private static string MakeValidEnumName(string name) {
        var validName = name.Replace(" ", "").Replace("-", "_").Replace(".", "_");

        if (char.IsDigit(validName[0])) {
            validName = "_" + validName;
        }

        return validName;
    }
}
#endif
