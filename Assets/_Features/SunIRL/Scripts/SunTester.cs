using UnityEngine;

public class SunTester: MonoBehaviour {

    void Start() {
        FindAnyObjectByType<SunManager>().ToggleUI(true);
    }

}
