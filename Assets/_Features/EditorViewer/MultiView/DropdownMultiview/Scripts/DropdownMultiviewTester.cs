using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownMultiviewTester : MonoBehaviour
{
    public DropdownMultiview dropdown;

    void Start() {
        var variants = new List<MapVariant>
        {
            new GameObject("awa").AddComponent<MapVariant>(),
            new GameObject("awaa").AddComponent<MapVariant>(),
            new GameObject("awaaa").AddComponent<MapVariant>(),
            new GameObject("awaaaa").AddComponent<MapVariant>(),
        };
        foreach (var variant in variants) {
            variant.Name = variant.gameObject.GetInstanceID().ToString();
        }

        dropdown.SetupMultiview(variants);
    }
}
