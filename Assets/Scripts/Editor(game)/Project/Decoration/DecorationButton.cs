using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationButton : MonoBehaviour {

    DecorationPreset decorationPreset;
    Button button;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(DecorationPreset deco) {
        decorationPreset = deco;
        button.onClick.AddListener(OnButtonClick);
        text.text = decorationPreset.Name;
    }

    void OnButtonClick() {
        DecorationManager.Instance.SetActiveDecorationPreset(decorationPreset);
        UImanager.Instance.ShowUI(UIType.DecorationPopUp);
    }
}
