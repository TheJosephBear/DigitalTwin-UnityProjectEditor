using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationButton : MonoBehaviour {

    public DecorationPreset decorationPreset;
    Button button;
    [SerializeField] private TextMeshProUGUI text;
    public Outline SelectedOutline;
    bool selected = false;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(DecorationPreset deco) {
        decorationPreset = deco;
        button.onClick.AddListener(OnButtonClick);
        button.onClick.AddListener(() => FindAnyObjectByType<DecorationUI>().RefreshDecorationButtonList()); // Nechuárna ale nevím jak jinak aktualizovat ty outline všech tlaèítek
        text.text = decorationPreset.Name;
    }

    void OnButtonClick() {
        AudioManager.Instance.PlaySound(SoundType.click);
        if (!selected) {
            GetSelected();
        } else {
            GetUnselected();
        }
    }

    public void GetSelected() {
        DecorationManager.Instance.SetActiveDecorationPreset(decorationPreset);
        SelectedOutline.enabled = true;
    }

    public void GetUnselected() {
        SelectedOutline.enabled = false;    
    }

    public void onOtevrit() {
        GetSelected();
        DecorationManager.Instance.ShowDecorationVariantEditorMenu();   
    }

    public void onPrejmenovat() {
        GetSelected();
        PopUpTextInput.Instance.AskForInput("Pøejmenovat dekoraci", (input) => {
            if (input!=null) 
                DecorationManager.Instance.RenameSelectedDecoration(input);
        });
    }

    public void onOdstranit() {
        GetSelected();
        DecorationManager.Instance.DeleteSelectedDecoration();
    }
}
