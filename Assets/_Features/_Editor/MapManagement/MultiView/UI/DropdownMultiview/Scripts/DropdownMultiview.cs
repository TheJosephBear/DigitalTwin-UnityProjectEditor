using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEditor.Progress;

public class DropdownMultiview : MonoBehaviour {
    private DropdownOriginal dropdown;
    private List<DropdownMultiviewItem> multiviewItems = new();

    private MapVariant currentlyLocked = null;

    private void Awake() {
        dropdown = GetComponent<DropdownOriginal>();
    }

    public void SetupMultiview(List<MapVariant> variants) {
        List<string> labels = variants.ConvertAll(v => v.Name);

        dropdown.Setup(labels, OnItemSelected);

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
        multi.ConnectToOriginalItem(original.GetComponent<DropdownOriginalItem>());
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

        // Select the item also
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
    private void OnItemSelected(string selectedLabel) {
        Debug.Log($"Selected: {selectedLabel}");
    }
}
