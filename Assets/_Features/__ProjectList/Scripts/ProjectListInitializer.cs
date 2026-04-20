using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectListInitializer : MonoBehaviour, Iinitializer {
    public void Initialize() {
        UIManager.Instance.ShowUI(UIType.ProjectsList);

    }


    public void Unload() {

    }

}
