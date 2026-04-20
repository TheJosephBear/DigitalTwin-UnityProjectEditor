using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginSceneInitializer : MonoBehaviour, Iinitializer {
    public void Initialize() {
        UIManager.Instance.ShowUI(UIType.Login);

    }

    public void Unload() {

    }
}