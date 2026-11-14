using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpInputUI : UIBehaviour {

    public TextMeshProUGUI messageLabel;
    public TMP_InputField inputField;

    Action<string> onInputSubmitted;
    bool _isOpen;
    int _lastSubmitFrame = -1;

    protected override void Awake() {
        base.Awake();
        inputField.onSubmit.AddListener(_ => OnEnterPressed());
    }

    public void AskForInput(string message, Action<string> callback) {
        messageLabel.text = message;
        inputField.text = "";
        onInputSubmitted = callback;
        _isOpen = true;
        InputFieldNavigation.Focus(inputField);
    }

    public void ShowCopyableText(string windowMessage, string text) {
        messageLabel.text = windowMessage;
        inputField.text = text;
        onInputSubmitted = null;
        _isOpen = true;
        InputFieldNavigation.Focus(inputField); // Selects the text, ready to copy
    }

    void Update() {
        // ESC closes the popup like the cancel button
        if (_isOpen && Input.GetKeyDown(KeyCode.Escape) && GetComponent<Canvas>().enabled) {
            OnCancelButtonClicked();
        }
    }

    void OnEnterPressed() {
        // One Enter can come both from the field and from the UI submit action, and must not submit twice
        if (!_isOpen || _lastSubmitFrame == Time.frameCount) return;
        _lastSubmitFrame = Time.frameCount;
        OnSubmitButtonClicked();
    }

    public void OnSubmitButtonClicked() {
        AudioManager.Instance.PlaySound(SoundType.click);
        _isOpen = false;
        string userInput = inputField.text;
        onInputSubmitted?.Invoke(userInput);
        UIManager.Instance.HideUI(UIType.PopUpInputUI);
    }

    public void OnCancelButtonClicked() {
        _isOpen = false;
        AudioManager.Instance.PlaySound(SoundType.click);
        onInputSubmitted?.Invoke(null); 
        UIManager.Instance.HideUI(UIType.PopUpInputUI);
    }
}
