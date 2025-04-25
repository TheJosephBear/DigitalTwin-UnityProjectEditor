using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMapLocalizationMode : MonoBehaviour, IClickable {

    public void OnClick() {

    }

    public void OnClickDown() {
        GeoMapManager.Instance.ToggleGeoMapControl();
    }

    public void OnClickUp() {

    }

    public void OnHover() {

    }

    public void OnUnhover() {

    }


}
