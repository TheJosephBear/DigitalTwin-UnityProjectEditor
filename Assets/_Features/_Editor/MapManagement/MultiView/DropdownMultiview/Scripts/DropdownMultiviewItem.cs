using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DropdownMultiviewItem : MonoBehaviour {
    public Button lockButton;
    Image lockVisual;
    public MapVariant mapVariant;

    private DropdownMultiview dropdownMultiview;

    public Action onSelectableClick;

    public void Setup(MapVariant variant, DropdownMultiview dropdownMultiview) {
        lockVisual = lockButton.image;
        this.mapVariant = variant;
        this.dropdownMultiview = dropdownMultiview;

        lockButton.onClick.RemoveAllListeners();
        lockButton.onClick.AddListener(() => ToggleLock());

        UpdateLockVisual();
    }

    public void ConnectToOriginalItem(DropdownOriginalItem originalItem) {
        originalItem.IsOverriden = true;
        originalItem.button.onClick.RemoveAllListeners(); // the original function still gets called.
        originalItem.button.onClick.AddListener(() => {
            if (!mapVariant.IsLocked && dropdownMultiview.IsAnythingLocked()) {
            } else if (mapVariant.IsLocked || !mapVariant.IsLocked && !dropdownMultiview.IsAnythingLocked()) {
                originalItem.onClick.Invoke();
                onSelectableClick?.Invoke();
            }
        });
    }

    private void ToggleLock() {
        dropdownMultiview.OnLockToggled(this);
    }

    public void UpdateLockVisual() {
        lockVisual.color = mapVariant.IsLocked ? Color.black : Color.grey;
    }
}
