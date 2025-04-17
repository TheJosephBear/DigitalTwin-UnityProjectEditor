using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TwoMapsUI : UIBehaviour {

    public TMP_Dropdown DropDownPrimary;
    public TMP_Dropdown DropDownSecondary;

    private List<MapVariant> _mapVariants = new List<MapVariant>();
    private MapVariant _lockedVariant = null;

    public void Initialize() {
        UpdateDropDown();
        if (_mapVariants.Count > 0) {
            MapDisplayManager.Instance.ShowVariant(_mapVariants[0], MapPriority.Secondary);
        }
    }

    public void UpdateDropDown() {
        _mapVariants = MapManager.Instance.GetVariants();
        DropDownPrimary.ClearOptions();
        DropDownSecondary.ClearOptions();
        List<string> options = new List<string>();

        foreach (var variant in _mapVariants) {
            options.Add(variant.ModelAsset.ModelID);
        }

        DropDownPrimary.AddOptions(options);
        DropDownSecondary.AddOptions(options);
        DropDownPrimary.onValueChanged.AddListener(OnMapVariantSelectedPrimary);
        DropDownSecondary.onValueChanged.AddListener(OnMapVariantSelectedSecondary);
        HookDropdownItemLogic(DropDownPrimary);
        HookDropdownItemLogic(DropDownSecondary);

    }

    public void HookDropdownItemLogic(TMP_Dropdown dropdown) {
        StartCoroutine(SetupDropdownItemsAfterExpand(dropdown));
    }

    private IEnumerator SetupDropdownItemsAfterExpand(TMP_Dropdown dropdown) {
        yield return new WaitForEndOfFrame(); // Wait until dropdown has expanded

        var scroll = dropdown.template.GetComponentInChildren<ScrollRect>();
        if (scroll == null) yield break;

        Transform content = scroll.content;

        for (int i = 0; i < content.childCount; i++) {
            var itemGO = content.GetChild(i).gameObject;
            var itemScript = itemGO.GetComponent<MultiviewDropdownItem>();

            if (itemScript != null && i < _mapVariants.Count) {
                MapVariant variant = _mapVariants[i];
                itemScript.Initialize(variant);
            }
        }
    }

    public void OnMapLockToggle(MapVariant toggledVariant) {
        if (_lockedVariant == toggledVariant) {
            _lockedVariant.IsLocked = false;
            _lockedVariant = null;
        } else {
            if (_lockedVariant != null)
                _lockedVariant.IsLocked = false;
            toggledVariant.IsLocked = true;
            _lockedVariant = toggledVariant;
        }

        UpdateDropdownItemVisuals(DropDownPrimary);
        UpdateDropdownItemVisuals(DropDownSecondary);
    }

    private void UpdateDropdownItemVisuals(TMP_Dropdown dropdown) {
        var scroll = dropdown.template.GetComponentInChildren<ScrollRect>();
        if (scroll == null) return;

        Transform content = scroll.content;
        int mapVariantIdx = 0;
        for (int i = 0; i < content.childCount; i++) {
            var itemGO = content.GetChild(i).gameObject;
            var itemScript = itemGO.GetComponent<MultiviewDropdownItem>();

            if (itemScript != null && mapVariantIdx < _mapVariants.Count) {
                bool shouldGray = _lockedVariant != null && _lockedVariant != _mapVariants[mapVariantIdx];
                itemScript.LockButton.image.color = shouldGray ? Color.gray : Color.black;
                mapVariantIdx++;
            }
        }
    }


    private void OnMapVariantSelectedPrimary(int index) {
        if (index >= 0 && index < _mapVariants.Count) {
            MapVariant selectedVariant = _mapVariants[index];
            MapDisplayManager.Instance.ShowVariant(_mapVariants[index], MapPriority.Primary);
        } else {
            print("Selected index out of bounds for map variants.");
        }
    }

    private void OnMapVariantSelectedSecondary(int index) {
        if (index >= 0 && index < _mapVariants.Count) {
            MapVariant selectedVariant = _mapVariants[index];
            MapDisplayManager.Instance.ShowVariant(_mapVariants[index], MapPriority.Secondary);
        } else {
            print("Selected index out of bounds for map variants.");
        }
    }

}
