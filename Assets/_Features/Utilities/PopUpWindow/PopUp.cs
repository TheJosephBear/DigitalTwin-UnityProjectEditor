using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUp : Singleton<PopUp> {

    Action<string> onInputSubmitted;


    protected override void Awake() {
        base.Awake();
    }


    // Only shows message
    public void ShowPopUpWindow(string text) {
        UImanager.Instance.ShowUI(UIType.PopUpMessageUI);   
        UImanager.Instance.GetActiveUIscript().GetComponent<PopUpMessageUI>().SetText(text);
    }


    public void AskForInput(string message, Action<string> callback) {
        UImanager.Instance.ShowUI(UIType.PopUpInputUI);
        UImanager.Instance.GetActiveUIscript().GetComponent<PopUpInputUI>().AskForInput(message, (input) => {
            onInputSubmitted = callback;
        });
    }

    public void ShowCopyableText(string message, string text) {
        UImanager.Instance.ShowUI(UIType.PopUpInputUI);
        UImanager.Instance.GetActiveUIscript().GetComponent<PopUpInputUI>().ShowCopyableText(message, text);
    }


}
