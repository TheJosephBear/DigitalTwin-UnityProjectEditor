using UnityEngine;

public class MapTester : MonoBehaviour {
    public void OnMapButton() {
        MapManager.Instance.ToggleMapUI(true);
    }
}