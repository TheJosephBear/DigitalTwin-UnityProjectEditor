using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiviewDropdownItem : MonoBehaviour {  

    public Button LockButton;
    public MapVariant MapVariantReff;

    public void Initialize(MapVariant mapVariant) { 
        MapVariantReff = mapVariant;
        LockButton.onClick.AddListener(OnLockClick);
    }

    public void OnLockClick() {
        FindAnyObjectByType<TwoMapsUI>().OnMapLockToggle(MapVariantReff);
    }

}
