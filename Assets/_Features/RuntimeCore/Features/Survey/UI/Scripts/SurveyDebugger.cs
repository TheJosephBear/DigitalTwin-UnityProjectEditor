using UnityEngine;

public class SurveyDebugger : MonoBehaviour, IInitializationListener {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
    }

    public void OnSceneInitialized() {
        if (SurveyManager.Instance == null) {
            gameObject.AddComponent<SurveyManager>();
        }
        FindAnyObjectByType<SurveyFlowManager>().EnterSurveyBuilding();
    }

    // Update is called once per frame
    void Update() {

    }
}
