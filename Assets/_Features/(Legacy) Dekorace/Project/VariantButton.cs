using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class VariantButton : MonoBehaviour {
    
    DecorationVariant variant;
    Button button;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(DecorationVariant vari) {
        variant = vari;
        button.onClick.AddListener(OnButtonClick);
  //      button.onClick.AddListener(() => FindAnyObjectByType<DecorationUI>().RefreshVariantButtonListSelection()); // Nechuárna ale nevím jak jinak aktualizovat ty outline všech tlačítek
        text.text = variant.Name;
    }

    void OnButtonClick() {
        AudioManager.Instance.PlaySound(SoundType.click);
    }

    public void onPridatDoSceny() {
        DecorationManager.Instance.SpawnVariant(variant);
    }

    public void onPrejmenovat() {
     /*   PopUpTextInput.Instance.AskForInput("Přejmenovat dekoraci", (input) => {
            if (input != null)
                DecorationManager.Instance.RenameVariant(variant, input);
        });*/
    }

    public void onOdstranit() {
        DecorationManager.Instance.DeleteVariant(variant);
    }
}
