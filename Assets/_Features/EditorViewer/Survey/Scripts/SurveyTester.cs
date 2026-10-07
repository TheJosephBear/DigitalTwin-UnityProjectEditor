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
            "{\"Name\":\"\",\"Description\":\"\",\"Questions\":[{\"rid\":1000}],\"references\":{\"version\":2,\"RefIds\":[{\"rid\":1000,\"type\":{\"class\":\"SerializableQuestion\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Id\":0,\"Title\":\"\",\"Description\":\"\",\"ViewPointId\":\"\",\"ImageId\":\"\",\"QuestionType\":7,\"Answers\":[{\"rid\":1001},{\"rid\":1002},{\"rid\":1003}]}},{\"rid\":1001,\"type\":{\"class\":\"AnswerImage\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Idx\":0,\"Text\":\"\",\"IsOther\":false,\"ImageID\":\"082fabd8a30104882857fc534135753d\"}},{\"rid\":1002,\"type\":{\"class\":\"AnswerImage\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Idx\":2,\"Text\":\"\",\"IsOther\":false,\"ImageID\":\"081605f5fc9af223344f6782c6b19d3a\"}},{\"rid\":1003,\"type\":{\"class\":\"AnswerImage\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Idx\":2,\"Text\":\"\",\"IsOther\":false,\"ImageID\":\"\"}}]}}";
        
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
