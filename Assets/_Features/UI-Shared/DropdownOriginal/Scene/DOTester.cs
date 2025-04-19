using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTester : MonoBehaviour {
    public DropdownOriginal dropdown;

    void Start() {
        List<string> items = new() { "Warrior", "Mage", "Rogue" };

        dropdown.Setup(items, OnItemSelected);
    }

    void OnItemSelected(string value) {
        Debug.Log($"Selected: {value}");
    }
}