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
        if (_mapVariants.Count > 0) {
            MapDisplayManager.Instance.ShowVariant(_mapVariants[0], MapPriority.Secondary);
        }
    }

    public void OnLeave() {
        MapDisplayManager.Instance.Exit();
    }

    public void UpdateDropDowns() {
        List<MapVariant> mapVariants = MapManager.Instance.GetVariants();
        // for now use IDs for names...
        foreach (MapVariant item in mapVariants) {
            item.Name = item.ModelAsset.ModelID;
        }

        DropDownPrimary.SetupMultiview(mapVariants);
        DropDownSecondary.SetupMultiview(mapVariants);

        /*
        // Cache selected indexes
        int selectedPrimary = DropDownPrimary.value;
        int selectedSecondary = DropDownSecondary.value;

        // Prevent event stacking
        DropDownPrimary.onValueChanged.RemoveAllListeners();
        DropDownSecondary.onValueChanged.RemoveAllListeners();

        DropDownPrimary.ClearOptions();
        DropDownSecondary.ClearOptions();

        _mapVariants = MapManager.Instance.GetVariants();
        List<string> options = new List<string>(_mapVariants.Count);
        foreach (var variant in _mapVariants) {
            options.Add(variant.ModelAsset.ModelID);
        }

        DropDownPrimary.AddOptions(options);
        DropDownSecondary.AddOptions(options);

        DropDownPrimary.onValueChanged.AddListener(OnMapVariantSelectedPrimary);
        DropDownSecondary.onValueChanged.AddListener(OnMapVariantSelectedSecondary);

        // Restore selected values if still valid
        DropDownPrimary.value = Mathf.Clamp(selectedPrimary, 0, _mapVariants.Count - 1);
        DropDownSecondary.value = Mathf.Clamp(selectedSecondary, 0, _mapVariants.Count - 1);

        // Initialize dropdown UI items AFTER frame to ensure instantiation
        StartCoroutine(SetupDropdownItemsAfterExpand(DropDownPrimary));
        StartCoroutine(SetupDropdownItemsAfterExpand(DropDownSecondary));
        */
    }

    private IEnumerator SetupDropdownItemsAfterExpand(TMP_Dropdown dropdown) {
        // Wait until dropdown is populated & visible
        dropdown.Show();
        yield return new WaitForEndOfFrame();

        var scroll = dropdown.template.GetComponentInChildren<ScrollRect>();
        if (scroll == null) yield break;

        Transform content = scroll.content;

        // Ensure dropdown options are visible before trying to access instantiated items
        yield return new WaitForSeconds(0.05f); // ensures UI has caught up

        for (int i = 0; i < content.childCount && i < _mapVariants.Count; i++) {
            GameObject itemGO = content.GetChild(i).gameObject;
            DropdownMultiviewItem itemScript = itemGO.GetComponent<DropdownMultiviewItem>();
            if (itemScript != null) {
          //      itemScript.Initialize(_mapVariants[i]);
            }
        }

        // Close and reopen dropdown to reflect new visuals (optional)
        EventSystem.current.SetSelectedGameObject(null);
        dropdown.Hide(); // If accessible
    }

    public void OnMapLockToggle(MapVariant toggledVariant) {
        toggledVariant.IsLocked = !toggledVariant.IsLocked;
        UpdateDropDowns(); // this will now preserve selection
    }

    private void OnMapVariantSelectedPrimary(int index) {
        if (index >= 0 && index < _mapVariants.Count) {
            MapDisplayManager.Instance.ShowVariant(_mapVariants[index], MapPriority.Primary);
        } else {
            Debug.LogWarning("Primary index out of range.");
        }
    }

    private void OnMapVariantSelectedSecondary(int index) {
        if (index >= 0 && index < _mapVariants.Count) {
            MapDisplayManager.Instance.ShowVariant(_mapVariants[index], MapPriority.Secondary);
        } else {
            Debug.LogWarning("Secondary index out of range.");
        }
    }
}
