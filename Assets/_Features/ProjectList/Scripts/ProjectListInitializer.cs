using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectListInitializer : MonoBehaviour, Iinitializer {
    public void Initialize() {
        FindAnyObjectByType<ProjectListUINew>().Initialize();
    }


    public void Unload() {

    }

}
