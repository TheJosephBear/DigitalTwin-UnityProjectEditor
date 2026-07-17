using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewHUDButton : MonoBehaviour {

    public ViewPointUI UIreff;
    public ViewPoint ViewPointRefference;
    public Image ButtonImageRef;

    public void Initialize(ViewPointUI ui, ViewPoint vp) {
        UIreff = ui;
        ViewPointRefference = vp;
        ToggleVisual(ViewManager.Instance.GetActiveViewPoint() == ViewPointRefference);
    }

    public void OnClick() {
        UIreff.OnHUDButtonClick(ViewPointRefference);
    }

    public void ToggleVisual(bool toggleOn) {
        print(toggleOn);
        string color = "#FFFFFF";
        if (toggleOn) {
            color = "#FF92FE";
        }

        if (ColorUtility.TryParseHtmlString(color, out Color newColor)) {
            ButtonImageRef.color = newColor;
        }
    }

}
