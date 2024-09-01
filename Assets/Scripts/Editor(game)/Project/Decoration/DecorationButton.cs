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

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(DecorationPreset deco) {
        decorationPreset = deco;
        button.onClick.AddListener(OnButtonClick);
        button.onClick.AddListener(() => FindAnyObjectByType<DecorationUI>().RefreshDecorationButtonList()); // Nechuù·rna ale nevÌm jak jinak aktualizovat ty outline vöech tlaËÌtek
        text.text = decorationPreset.Name;
    }

    void OnButtonClick() {
        AudioManager.Instance.PlaySound(SoundType.click);
        GetSelected();
    }

    public void GetSelected() {
        DecorationManager.Instance.SetActiveDecorationPreset(decorationPreset);
        SelectedOutline.enabled = true;
    }

    public void GetUnselected() {
        SelectedOutline.enabled = false;    
    }
}
