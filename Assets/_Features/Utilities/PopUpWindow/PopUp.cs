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
        UIManager.Instance.ShowUI(UIType.PopUpMessageUI);   
        UIManager.Instance.GetActiveUIscript().GetComponent<PopUpMessageUI>().SetText(text);
    }

    public void AskForInput(string message, Action<string> callback) {
        UIManager.Instance.ShowUI(UIType.PopUpInputUI);
        UIManager.Instance.GetActiveUIscript().GetComponent<PopUpInputUI>().AskForInput(message, callback);
    }

    public void ShowCopyableText(string message, string text) {
        UIManager.Instance.ShowUI(UIType.PopUpInputUI);
        UIManager.Instance.GetActiveUIscript().GetComponent<PopUpInputUI>().ShowCopyableText(message, text);
    }
}
