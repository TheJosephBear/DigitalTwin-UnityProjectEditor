using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginSceneInitializer : MonoBehaviour, Iinitializer {
    public void Initialize() {

    }

    public void StartRunning() {
        UIManager.Instance.ShowUI(UIType.Login);
    }

    public void Unload() {

    }
}