using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer
{
    public void Initialize() {
        UImanager.Instance.ShowUI(UIType.EditorHUD);
        Project.Instance.OpenProject(ProjectListManager.Instance.selectedProjectRefference);
    }

    public void StartRunning() {

    }

    public void Unload() {

    }

}
