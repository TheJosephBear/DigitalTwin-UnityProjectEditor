using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewHUDButton : MonoBehaviour {

    public ViewPointUI UIreff;
    public ViewPoint ViewPointRefference;

    public void Initialize(ViewPointUI ui, ViewPoint vp) {
        UIreff = ui;
        ViewPointRefference = vp;
    }

    public void OnClick() {
        UIreff.OnHUDButtonClick(ViewPointRefference);
    }

}
