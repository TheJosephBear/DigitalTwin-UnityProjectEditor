using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUp : Singleton<PopUp> {

    public GameObject PopUpPrefab;

    protected override void Awake() {
        base.Awake();
    }

    public void ShowPopUpWindow(string text) {
        PopUpWindow pw = Instantiate(PopUpPrefab).GetComponent<PopUpWindow>();
        pw.SetText(text);
    }
    
}
