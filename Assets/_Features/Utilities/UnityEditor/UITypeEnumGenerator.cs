using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class UITypeEnumGenerator {
    // Create a menu item directly under "Tools" in the Unity Editor
    [MenuItem("Tools/Generate UIType Enum")]
    public static void GenerateEnum() {
        // Find all GameObjects in the scene with a UIBehaviour component
        var uiBehaviourObjects = GameObject.FindObjectsOfType<UIBehaviour>();

        // Prepare a list to hold the names for the enum
        var enumNames = new System.Collections.Generic.List<string>();

        foreach (var uiObject in uiBehaviourObjects) {
            if (uiObject != null) {
                enumNames.Add(uiObject.gameObject.name);
            }
        }

        // Create the enum file path
        string filePath = "Assets/Enums/UIType.cs";

        // Create the folder if it doesn't exist
        if (!Directory.Exists("Assets/Enums")) {
            Directory.CreateDirectory("Assets/Enums");
        }

        using (StreamWriter writer = new StreamWriter(filePath, false)) {
            writer.WriteLine("public enum UIType");
            writer.WriteLine("{");

            // Write enum entries from the UIBehaviour names
            foreach (var name in enumNames) {
                // Enum names must be valid C# identifiers, so ensure we use a valid format
                string validEnumName = MakeValidEnumName(name);
                writer.WriteLine($"    {validEnumName} = {enumNames.IndexOf(name)},");
            }

            writer.WriteLine("}");
        }

        // Refresh the editor to recognize the new script
        AssetDatabase.Refresh();
        Debug.Log($"Enum 'UIType' generated at {filePath}");
    }

    private static string MakeValidEnumName(string name) {
        // Replace any invalid characters for enum names (like spaces or special characters)
        var validName = name.Replace(" ", "").Replace("-", "_").Replace(".", "_");
        return validName;
    }
}
