using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEditor.Progress;
using System.Reflection;

public class DropdownOriginal : MonoBehaviour {
    [Header("UI Refs")]
    public GameObject itemPrefab;
    public GameObject TempButton;
    public Transform itemParent;

    private List<DropdownOriginalItem> items = new();
    private DropdownOriginalItem selectedItem;
    private Action<string> onValueChanged;
    private bool isExpanded = false;

    public void Setup(List<string> options, Action<string> onValueChanged = null) {
        DestroyImmediate(TempButton);
        ClearItems();
        this.onValueChanged = onValueChanged;

        for (int i = 0; i < options.Count; i++) {
            string label = options[i];
            var itemGO = Instantiate(itemPrefab, itemParent);
            var item = itemGO.GetComponent<DropdownOriginalItem>();

            int index = i; // capture index correctly for lambda
            item.Setup(label, () => OnItemSelected(index));
            items.Add(item);
        }

        if (items.Count > 0) {
            selectedItem = items[0];
            // Replace selected item's callback with toggle handler
            selectedItem.Setup(selectedItem.GetLabel(), ToggleDropdown);
            onValueChanged?.Invoke(selectedItem.GetLabel());
            UpdateVisibleItems();
        }
    }

    private void ClearItems() {
        foreach (var item in items)
            if (item != null)
                Destroy(item.gameObject);
        items.Clear();
        selectedItem = null;
    }

    public void OnItemSelected(int selectedIndex) {
        string selectedLabel = items[selectedIndex].GetLabel();

        SwapItems(0, selectedIndex);
        selectedItem = items[0];
        selectedItem.Setup(selectedLabel, ToggleDropdown);

        for (int i = 1; i < items.Count; i++) {
            // I have to save it to a new variable because otherwise it puts in the "i" variable refference
            // and not a number, so it calls i==3 when the button is pressed, pretty stupid if u ask me
            int correctIndex = i;
            items[i].Setup(items[i].GetLabel(), () => OnItemSelected(correctIndex));
        }

        onValueChanged?.Invoke(selectedLabel);
        ToggleDropdown(); 
    }

    private void ToggleDropdown() {
        isExpanded = !isExpanded;
        UpdateVisibleItems();
    }

    private void UpdateVisibleItems() {
        for (int i = 0; i < items.Count; i++) {
            items[i].gameObject.SetActive(isExpanded || i == 0);
        }
    }

    private void SwapItems(int a, int b) {
        // Swap list
        var temp = items[a];
        items[a] = items[b];
        items[b] = temp;

        // Swap positions in hierarchy
        items[a].transform.SetSiblingIndex(a);
        items[b].transform.SetSiblingIndex(b);
    }
}
