using UnityEngine;
using System.Collections.Generic;
using System;

public class DropdownOriginal : MonoBehaviour {
    [Header("UI Refs")]
    public GameObject itemPrefab;
    public GameObject TempButton;
    public Transform itemParent; 

    private List<DropdownOriginalItem> items = new();
    private DropdownOriginalItem selectedItem; 
    Action<int> onIndexChanged;
    private bool isExpanded = false;

    public void Setup(List<string> options, Action<int> onIndexChanged = null) { 
        DestroyImmediate(TempButton);
        ClearItems();
        this.onIndexChanged = onIndexChanged;

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
            selectedItem.Setup(selectedItem.GetLabel(), ToggleDropdown);
            selectedItem.SetSelected(true);
            onIndexChanged?.Invoke(0); 
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
        selectedItem = items[selectedIndex];

        onIndexChanged?.Invoke(selectedIndex);
        ToggleDropdown();

        for (int i = 0; i < items.Count; i++) {
            items[i].SetSelected(i == selectedIndex);
        }

        UpdateVisibleItems();
    }



    private void ToggleDropdown() {
        isExpanded = !isExpanded;
        UpdateVisibleItems();
    }

    private void UpdateVisibleItems() {
        if (isExpanded) {
            selectedItem.transform.SetSiblingIndex(0);

            for (int i = 0; i < items.Count; i++) {
                items[i].gameObject.SetActive(true);
            }
        } else {
            for (int i = 0; i < items.Count; i++) {
                items[i].gameObject.SetActive(items[i] == selectedItem);
            }
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

    public int GetSelectedIndex() => items.IndexOf(selectedItem);

}
