using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpTextInput : Singleton<PopUpTextInput> {

    public GameObject canvas;
    public TextMeshProUGUI messageLabel;
    public TMP_InputField inputField;

    Action<string> onInputSubmitted;


    protected override void Awake() {
        base.Awake();
        canvas.SetActive(false);
    }

    public void AskForInput(string message, Action<string> callback) { 
        canvas.SetActive(true);
        messageLabel.text = message;
        inputField.text = "";
        onInputSubmitted = callback;
    }

    public void OnSubmitButtonClicked() {
        AudioManager.Instance.PlaySound(SoundType.click);
        string userInput = inputField.text;
        onInputSubmitted?.Invoke(userInput);
        canvas.SetActive(false);
    }

    public void OnCancelButtonClicked() {
        AudioManager.Instance.PlaySound(SoundType.click);
        onInputSubmitted?.Invoke(null);
        canvas.SetActive(false);
    }
}