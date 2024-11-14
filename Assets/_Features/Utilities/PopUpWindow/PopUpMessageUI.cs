using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpMessageUI : UIBehaviour {
    [SerializeField]
    TextMeshProUGUI text;

    public void Exit() {
        AudioManager.Instance.PlaySound(SoundType.click);
        Destroy(this.gameObject);
    }

    public void SetText(string text) {
        this.text.text = text;
    }
}
