using UnityEngine;

public class SurveyTester : MonoBehaviour {
    SurveyBuilder builder;

    private void Start() {
        builder = FindAnyObjectByType<SurveyBuilder>();
        
        Test();
    }

    void Test() {

    }


}
