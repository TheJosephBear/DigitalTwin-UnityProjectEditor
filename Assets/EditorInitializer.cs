using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorInitializer : MonoBehaviour, Iinitializer
{
    public void Initialize() {
        UImanager.Instance.ShowUI(UIType.EditorHUD);
    }

    public void StartRunning() {

    }

    public void Unload() {

    }

}
