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
        }

        StartCoroutine(WaitForInit());
    }

    void LoadFromJson() {
        string json = "";
        string kson = "" +
            "{\"Name\":\"\",\"Questions\":[{\"rid\":1000},{\"rid\":1001},{\"rid\":1002},{\"rid\":1003}],\"references\":{\"version\":2,\"RefIds\":[{\"rid\":1000,\"type\":{\"class\":\"SerializableGridQuestion\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Id\":0,\"Title\":\"prosimte na kolenou delej co mas tak jak to chci\",\"Description\":\"fakt uz to ztracim prosimmmmmm\",\"ViewPointId\":\"\",\"QuestionType\":5,\"Answers\":[],\"Rows\":[\"aa\",\"bb\"],\"Columns\":[\"a\",\"b\",\"c\"]}},{\"rid\":1001,\"type\":{\"class\":\"SerializableQuestion\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Id\":1,\"Title\":\"klasika\",\"Description\":\"klasicky popis\",\"ViewPointId\":\"\",\"QuestionType\":0,\"Answers\":[{\"Idx\":0,\"Text\":\"a\",\"IsOther\":false},{\"Idx\":1,\"Text\":\"b\",\"IsOther\":false},{\"Idx\":2,\"Text\":\"c\",\"IsOther\":false}]}},{\"rid\":1002,\"type\":{\"class\":\"SerializableQuestion\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Id\":2,\"Title\":\"Klasika \",\"Description\":\"checkuj tuhle klasiku\",\"ViewPointId\":\"\",\"QuestionType\":0,\"Answers\":[{\"Idx\":0,\"Text\":\"aa\",\"IsOther\":false},{\"Idx\":1,\"Text\":\"bb\",\"IsOther\":false}]}},{\"rid\":1003,\"type\":{\"class\":\"SerializableQuestion\",\"ns\":\"SurveySystem\",\"asm\":\"Assembly-CSharp\"},\"data\":{\"Id\":3,\"Title\":\"Klasicky check\",\"Description\":\"zabrzd zadrz\",\"ViewPointId\":\"\",\"QuestionType\":1,\"Answers\":[{\"Idx\":0,\"Text\":\"a\",\"IsOther\":false},{\"Idx\":1,\"Text\":\"b\",\"IsOther\":false}]}}]}}";
        
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
