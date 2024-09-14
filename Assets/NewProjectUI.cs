using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewProjectUI : UIBehaviour {

    public TMP_InputField NameInputText;

    public void onCreate() {
        AudioManager.Instance.PlaySound(SoundType.click);
        if (string.IsNullOrWhiteSpace(NameInputText.text)) { 
            return;
        }
        ProjectListManager.Instance.CreateNewProject(NameInputText.text);
        UImanager.Instance.HideUI(UIType.NewProject);
    }

    public override void Hide() {
        canvas.SetActive(false);
    }

    public override void Show() {
        canvas.SetActive(true);
    }

}
