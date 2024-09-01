using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectSettingsUI : UIBehaviour {

    public GameObject canvas;

    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.HideUI(UIType.ProjectSettings);
    }

    public override void Show() {
        canvas.SetActive(true);
    }

    public override void Hide() {
        canvas.SetActive(false);
    }
}
