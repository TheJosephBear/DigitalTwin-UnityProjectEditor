using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpAreYouSureUI : MonoBehaviour {

    public TextMeshProUGUI MessageTextRef;
    Action<bool> onInputSubmitted;

    public void AskForInput(string text, Action<bool> callback) {
        MessageTextRef.text = text;
        onInputSubmitted = callback;
    }

    public void OnSubmitButtonClicked() {
        onInputSubmitted?.Invoke(true);
        Destroy(gameObject);
    }

    public void OnCancelButtonClicked() {
        onInputSubmitted?.Invoke(false);
        Destroy(gameObject);
    }

}
