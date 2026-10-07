using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownOriginalItem : MonoBehaviour
{
    public Button button;
    public GameObject IsSelectedVisualizer;
    public TMP_Text label;

    public System.Action onClick;
    public bool IsOverriden = false;

    public void Setup(string labelText, System.Action onClick) {
        label.text = labelText;
        if (IsOverriden) return;
        this.onClick = onClick;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { onClick?.Invoke();});
    }

    public void SetSelected(bool isSelected) {
        IsSelectedVisualizer.SetActive(isSelected ? true : false);
    }

    public string GetLabel() => label.text;
}
