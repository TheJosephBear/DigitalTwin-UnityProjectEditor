using UnityEngine;
using SurveySystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class SurveyTester : MonoBehaviour, IInitializationListener {

    public bool EditorMode = true;

    ViewManager _viewManager;

    public void OnSceneInitialized() {
        _viewManager = FindAnyObjectByType<ViewManager>();
        Test();
    }

    void Test() {
        if (_viewManager == null) {
            Debug.LogError("View manager could not be found in scene!");
        }
        _viewManager.CreateNewViewPoint(updateUI: false);
        _viewManager.CreateNewViewPoint(updateUI: false);
        _viewManager.CreateNewViewPoint(updateUI: false);
        _viewManager.CreateNewViewPoint(updateUI: false);

        foreach (ViewPoint vp in _viewManager.GetViewPoints()) {
            vp.gameObject.transform.Rotate(new Vector3(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180)));
            vp.gameObject.transform.position = (new Vector3(Random.Range(100, 380), Random.Range(180, 380), Random.Range(180, 380)));
        }

        StartCoroutine(WaitForInit());
    }

    void LoadFromJson() {
        string json = "";
        string kson = "" +
            "{\"Name\":\"\",\"Questions\":[{\"rid\":1000}],\"references\":{\"version\":2,\"RefIds\":[{\"rid\":1000,\"type\":{\"class\":\"SerializableQuestion\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Id\":0,\"Title\":\"Q\",\"Description\":\"P\",\"ViewPointId\":\"\",\"ImageId\":\"39406f047498d2c811934e4e6e470e84\",\"QuestionType\":7,\"Answers\":[{\"rid\":1001},{\"rid\":1002},{\"rid\":1003}]}},{\"rid\":1001,\"type\":{\"class\":\"AnswerImage\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Idx\":0,\"Text\":\"\",\"IsOther\":false,\"ImageID\":\"7433b658c5cc68509192cbe48749b5af\"}},{\"rid\":1002,\"type\":{\"class\":\"AnswerImage\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Idx\":1,\"Text\":\"\",\"IsOther\":false,\"ImageID\":\"51824b6579920c5624f1e88869d550e6\"}},{\"rid\":1003,\"type\":{\"class\":\"AnswerImage\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Idx\":2,\"Text\":\"\",\"IsOther\":false,\"ImageID\":\"c9ff4eaf2a9cecc338b67e0c92085958\"}}]}}";
        
        SurveyManager.Instance.SetSurveyJson(kson);
        if (EditorMode) {
            SurveyManager.Instance.EnterSurveyBuilding(debug: true);
        } else {
            SurveyManager.Instance.EnterSurveyViewing(debug: true);
        }
    }

    // Waiting for survey manager to instantiate his stuff
    // In scenes used in the app this wait wont be needed
    IEnumerator WaitForInit() {
        yield return new WaitForSeconds(0.2f);
        //    SurveyManager.Instance.EnterSurveyBuilding();
        LoadFromJson();
    }


}
