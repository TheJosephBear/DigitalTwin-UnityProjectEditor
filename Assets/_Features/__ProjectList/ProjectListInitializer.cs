using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectListInitializer : MonoBehaviour, Iinitializer {
    public void Initialize() {

    }

    public void StartRunning() {
        UImanager.Instance.ShowUI(UIType.ProjectsList);
    }

    public void Unload() {

    }

}
