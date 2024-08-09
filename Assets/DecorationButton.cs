using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationButton : MonoBehaviour {

    Decoration decoration;
    Button button;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(Decoration decorationInstance) {
        decoration = decorationInstance;
        button.onClick.AddListener(OnButtonClick);
        text.text = decoration.Name;
    }

    private void OnButtonClick() {
        ObjectUploadingManager.Instance.SetActiveDecoration(decoration);
        UImanager.Instance.ShowUI(UIType.DecorationPopUp);
    }
}
