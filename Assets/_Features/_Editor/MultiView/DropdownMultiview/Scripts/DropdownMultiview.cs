using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class DropdownMultiview : MonoBehaviour {

    public MapPriority MapPriority;

    private DropdownOriginal dropdown;
    private List<DropdownMultiviewItem> multiviewItems = new();
    private List<MapVariant> _mapVariants;

    private MapVariant currentlyLocked = null;

    private void Awake() {
        dropdown = GetComponent<DropdownOriginal>();
    }

    public void SetupMultiview(List<MapVariant> variants) {
        _mapVariants = variants;
        List<string> labels = variants.ConvertAll(v => v.Name);

        dropdown.Setup(labels, OnItemSelectedByIndex);

        multiviewItems.Clear();

        for (int i = 0; i < dropdown.itemParent.transform.childCount; i++) {
            DropdownMultiviewItem item = dropdown.itemParent.transform.GetChild(i).GetComponent<DropdownMultiviewItem>();
            if (item != null) {
                item.Setup(variants[i], this);
                StartCoroutine(ConnectItemCoroutine(item, item.GetComponent<DropdownOriginalItem>()));
                multiviewItems.Add(item);
            }
        }

        var first = multiviewItems[0];
        if (first.mapVariant.IsLocked) {
            currentlyLocked = first.mapVariant;
        }

        foreach (var item in multiviewItems) {
            item.UpdateLockVisual();
        }
    }

    IEnumerator ConnectItemCoroutine(DropdownMultiviewItem multi, DropdownOriginalItem original) {
        yield return new WaitForEndOfFrame();
        multi.ConnectToOriginalItem(original);
    }

    public void OnLockToggled(DropdownMultiviewItem toggledItem) {
        if (IsAnythingLocked() && !toggledItem.mapVariant.IsLocked) return;

        foreach (DropdownMultiviewItem item in multiviewItems) {
            if (item != toggledItem) {
                item.mapVariant.IsLocked = false;
            } else {
                toggledItem.mapVariant.IsLocked = !toggledItem.mapVariant.IsLocked;
            }
        }

        foreach (var item in multiviewItems) {
            item.UpdateLockVisual();
        }

        toggledItem.GetComponent<DropdownOriginalItem>().button.onClick.Invoke();
    }

    public bool IsAnythingLocked() {
        foreach (DropdownMultiviewItem item in multiviewItems) {
            if (item.mapVariant.IsLocked) {
                return true;
            }
        }
        return false;
    }

    private void OnItemSelectedByIndex(int index) { 
        if (_mapVariants == null || index < 0 || index >= _mapVariants.Count) {
            Debug.LogWarning("Invalid index selected from dropdown.");
            return;
        }

        EditorManager.Instance.MultiViewManager.ShowVariant(_mapVariants[index], MapPriority);
    }
}
