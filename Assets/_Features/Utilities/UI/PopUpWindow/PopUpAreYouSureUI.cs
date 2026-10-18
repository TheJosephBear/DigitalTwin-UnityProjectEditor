using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UnsavedChangesChoice {
    SaveAndExit,
    Exit,
    Cancel
}

public class PopUpAreYouSureUI : MonoBehaviour {

    const float SaveVariantContainerWidth = 400f;

    public TextMeshProUGUI MessageTextRef;
    public RectTransform ContainerRef;
    public RectTransform SubmitButtonRef;
    public TextMeshProUGUI SubmitButtonTextRef;
    public GameObject SaveButtonRef; // Hidden unless asking about unsaved changes

    Action<bool> onInputSubmitted;
    Action<UnsavedChangesChoice> onChoiceSubmitted;

    public void AskForInput(string text, Action<bool> callback) {
        MessageTextRef.text = text;
        onInputSubmitted = callback;
    }

    // Variant with three buttons: Zrušit | Odejít | Uložit a odejít
    public void AskSaveBeforeExit(string text, Action<UnsavedChangesChoice> callback) {
        MessageTextRef.text = text;
        onChoiceSubmitted = callback;

        ContainerRef.sizeDelta = new Vector2(SaveVariantContainerWidth, ContainerRef.sizeDelta.y);
        SaveButtonRef.SetActive(true);

        // Submit button moves to the middle and exits without saving
        SubmitButtonRef.anchorMin = SubmitButtonRef.anchorMax = new Vector2(0.5f, 0f);
        SubmitButtonRef.anchoredPosition = new Vector2(0f, SubmitButtonRef.anchoredPosition.y);
        SubmitButtonTextRef.text = "Odejít";
    }

    public void OnSubmitButtonClicked() {
        onInputSubmitted?.Invoke(true);
        onChoiceSubmitted?.Invoke(UnsavedChangesChoice.Exit);
        Destroy(gameObject);
    }

    public void OnCancelButtonClicked() {
        onInputSubmitted?.Invoke(false);
        onChoiceSubmitted?.Invoke(UnsavedChangesChoice.Cancel);
        Destroy(gameObject);
    }

    public void OnSaveButtonClicked() {
        onChoiceSubmitted?.Invoke(UnsavedChangesChoice.SaveAndExit);
        Destroy(gameObject);
    }

}
