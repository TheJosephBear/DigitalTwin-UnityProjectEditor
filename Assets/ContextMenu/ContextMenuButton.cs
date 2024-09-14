using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ContextMenuButton : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI label;
    private Button button;

    public void Initialize(ContextMenuButtonEditor tempClass) {
        button = GetComponent<Button>();
        label.text = tempClass.text;
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() => {
            tempClass.onClickAction?.Invoke();
        });
    }
}
