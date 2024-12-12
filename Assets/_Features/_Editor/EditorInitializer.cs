using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer
{
    public void Initialize() {
        UImanager.Instance.ShowUI(UIType.EditorHUD);
     //   PopUp.Instance.ShowPopUpWindow("You have sucessfuly opened project " + ProjectManager.Instance.project.ProjectName);
    }

    public void StartRunning() {

    }

    public void Unload() {
        UImanager.Instance.HideUI(UIType.EditorHUD);
    }

    

}
