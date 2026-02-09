using UnityEngine;
using SurveySystem;

public class SurveyTester : MonoBehaviour {

    SurveySystem.SurveyBuilder _builder;

    private void Start() {
        _builder = FindAnyObjectByType<SurveySystem.SurveyBuilder>();
        
        Test();
    }

    void Test() {
        _builder.CreateNewSurvey();
    }


}
