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

    // Locked: the full icon color, unlocked: faded. The color itself comes from the IconTheme
    public void UpdateLockVisual() {
        float opacity = mapVariant.IsLocked ? 1f : 0.45f;
        if (lockVisual.TryGetComponent(out ThemedIcon themedIcon)) {
            themedIcon.Opacity = opacity;
        } else {
            Color color = IconTheme.Current != null ? IconTheme.Current.IconColor : Color.black;
            color.a *= opacity;
            lockVisual.color = color;
        }
    }
}
