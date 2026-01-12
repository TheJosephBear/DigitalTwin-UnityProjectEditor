using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectSettingsUI : UIBehaviour {


    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
  //      UIManager.Instance.HideUI(UIType.ProjectSettings);
    }

}
