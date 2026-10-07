using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MultiviewUI : UIBehaviour {

    public DropdownMultiview DropDownPrimary;
    public DropdownMultiview DropDownSecondary;

    private List<MapVariant> _mapVariants = new List<MapVariant>();

    public override void Show() {
        base.Show();
        Initialize();
    }

    public void Initialize() {
        UpdateDropDowns();
        if (_mapVariants != null && _mapVariants.Count > 0) {
            EditorManager.Instance.MultiViewManager.ShowVariant(_mapVariants[0], MapPriority.Secondary);
        }
    }

    public void OnLeave() {
        EditorManager.Instance.MultiViewManager.Exit();
    }

    public void UpdateDropDowns() {
        // FIX 1: Assign to the class-level list instead of a local variable
        _mapVariants = EditorManager.Instance.MapManager.GetVariants();

        if (DropDownPrimary != null) DropDownPrimary.SetupMultiview(_mapVariants);
        if (DropDownSecondary != null) DropDownSecondary.SetupMultiview(_mapVariants);
    }

    public void OnMapLockToggle(MapVariant toggledVariant) {
        if (toggledVariant == null) return;
        toggledVariant.IsLocked = !toggledVariant.IsLocked;
        UpdateDropDowns();
    }

    private void OnMapVariantSelectedPrimary(int index) {
        if (_mapVariants != null && index >= 0 && index < _mapVariants.Count) {
            EditorManager.Instance.MultiViewManager.ShowVariant(_mapVariants[index], MapPriority.Primary);
        } else {
            Debug.LogWarning("Primary index out of range.");
        }
    }

    private void OnMapVariantSelectedSecondary(int index) {
        if (_mapVariants != null && index >= 0 && index < _mapVariants.Count) {
            EditorManager.Instance.MultiViewManager.ShowVariant(_mapVariants[index], MapPriority.Secondary);
        } else {
            Debug.LogWarning("Secondary index out of range.");
        }
    }
}