using System.Collections.Generic;
using UnityEngine;

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
