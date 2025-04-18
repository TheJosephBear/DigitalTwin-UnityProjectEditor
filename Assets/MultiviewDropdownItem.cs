using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiviewDropdownItem : MonoBehaviour {  

    public Button LockButton;
    public MapVariant MapVariantReff;

    public void Initialize(MapVariant mapVariant) {
        print("lockbutton initialized");
        print($"Our name is {mapVariant.ModelAsset.ModelID}");
        print($"Our lockstate is {mapVariant.IsLocked}");
        MapVariantReff = mapVariant;
        LockButton.onClick.AddListener(OnLockClick);
        LockButton.image.color = MapVariantReff.IsLocked ? Color.black : Color.gray;
    }

    public void OnLockClick() {
        if(MapVariantReff == null) {
            print("Map variant isnt set on the lock button!");
            return;
        }

        FindAnyObjectByType<TwoMapsUI>().OnMapLockToggle(MapVariantReff);
    }

}
