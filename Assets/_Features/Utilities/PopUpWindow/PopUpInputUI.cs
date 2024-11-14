using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpInputUI : UIBehaviour {

    public TextMeshProUGUI messageLabel;
    public TMP_InputField inputField;

    Action<string> onInputSubmitted;

    public void AskForInput(string message, Action<string> callback) {
        messageLabel.text = message;
        inputField.text = "";
        onInputSubmitted = callback;
    }

    public void ShowCopyableText(string windowMessage, string text) {
        messageLabel.text = windowMessage;
        inputField.text = text;
    }

    public void OnSubmitButtonClicked() {
        AudioManager.Instance.PlaySound(SoundType.click);
        string userInput = inputField.text;
        onInputSubmitted?.Invoke(userInput); 
        UImanager.Instance.HideUI(UIType.PopUpInputUI);
    }

    public void OnCancelButtonClicked() {
        AudioManager.Instance.PlaySound(SoundType.click);
        onInputSubmitted?.Invoke(null); 
        UImanager.Instance.HideUI(UIType.PopUpInputUI);
    }
}
