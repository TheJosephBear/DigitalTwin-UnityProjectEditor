using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer
{
    public void Initialize() {
        UImanager.Instance.ShowUI(UIType.EditorHUD);
        UImanager.Instance.ShowUI(UIType.EditorObjectInfoUI);
        //   PopUp.Instance.ShowPopUpWindow("You have sucessfuly opened project " + ProjectManager.Instance.project.ProjectName);
    }

    public void StartRunning() {

     //   InterestPointManager.Instance.ToggleCameraPreview(false);
    }

    public void Unload() {
        UImanager.Instance.HideUI(UIType.EditorHUD);
        UImanager.Instance.HideUI(UIType.EditorObjectInfoUI);
    }

    

}
