using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Utilities {
    /// <summary>
    /// Destroys all GameObjects in the provided list and clears the list.
    /// </summary>
    
    
    /// <typeparam name="T">Type of the objects in the list, must be a Component.</typeparam>
    /// <param name="list">The list containing objects to destroy.</param>
    public static void DestroyAllGameObjects<T>(List<T> list) where T : Component {
        if (list == null) return;

        // Create a temporary list to avoid modifying the original list while iterating
        List<T> tempList = new List<T>(list);

        // Destroy all GameObjects
        foreach (var item in tempList) {
            if (item != null) {
                Object.Destroy(item.gameObject);
            }
        }

        // Clear the original list
        list.Clear();
    }

    public static string UniqueNameEnsure<T>(string name, List<T> objects) where T : EditorObjectBase {
        string baseName = name;
        string uniqueName = baseName;
        int copyNumber = 1;

        // Function to check if a name exists in the list
        bool NameExists(string checkName) =>
            objects.Any(obj => obj.Name == checkName);

        if (!NameExists(uniqueName)) {
            return uniqueName;
        }

        while (NameExists(uniqueName)) {
            // Check if the baseName ends with a numeric suffix in parentheses
            int lastIndexOfOpenParenthesis = baseName.LastIndexOf('(');
            int lastIndexOfCloseParenthesis = baseName.LastIndexOf(')');
            if (lastIndexOfOpenParenthesis != -1 && lastIndexOfCloseParenthesis == baseName.Length - 1) {
                string suffix = baseName.Substring(lastIndexOfOpenParenthesis + 1, lastIndexOfCloseParenthesis - lastIndexOfOpenParenthesis - 1);
                if (int.TryParse(suffix, out int existingNumber)) {
                    copyNumber = existingNumber + 1;
                    baseName = baseName.Substring(0, lastIndexOfOpenParenthesis).Trim();
                }
            }
            uniqueName = $"{baseName} ({copyNumber})";
            copyNumber++;
        }

        return uniqueName;
    }

    public static List<GameObject> GetDropdownItems(TMP_Dropdown dropdown) {
        var templateInstance = dropdown.template;
        var content = templateInstance.GetComponentInChildren<ScrollRect>()
                                      .content;

        List<GameObject> itemObjects = new List<GameObject>();

        foreach (Transform child in content) {
            itemObjects.Add(child.gameObject);
        }

        return itemObjects;
    }

    public static void KillAllChildren(this Transform parent) {
        for (int i = parent.childCount - 1; i >= 0; i--) {
            Transform child = parent.GetChild(i);
            if (Application.isPlaying)
                GameObject.Destroy(child.gameObject);
            else
                GameObject.DestroyImmediate(child.gameObject);
        }
    }
}
