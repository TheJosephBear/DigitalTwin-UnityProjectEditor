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
        string kson = "{\"Name\":\"\",\"Questions\":[{\"Id\":0,\"Title\":\"aaaa\",\"Description\":\"\",\"ViewPointId\":\"\",\"QuestionType\":0,\"Answers\":[{\"Idx\":0,\"Text\":\"asdasd\",\"IsOther\":false},{\"Idx\":1,\"Text\":\"asdasd\",\"IsOther\":false}]},{\"Id\":1,\"Title\":\"fdgdgg\",\"Description\":\"\",\"ViewPointId\":\"\",\"QuestionType\":1,\"Answers\":[{\"Idx\":0,\"Text\":\"hfgjghj\",\"IsOther\":false},{\"Idx\":1,\"Text\":\"fghfgh\",\"IsOther\":false}]},{\"Id\":2,\"Title\":\"paragrap\",\"Description\":\"\",\"ViewPointId\":\"\",\"QuestionType\":3,\"Answers\":[{\"Idx\":0,\"Text\":\"\",\"IsOther\":true},{\"Idx\":1,\"Text\":\"\",\"IsOther\":false}]},{\"Id\":3,\"Title\":\"lin\",\"Description\":\"\",\"ViewPointId\":\"\",\"QuestionType\":8,\"Answers\":[{\"Idx\":0,\"Text\":\"aaa\",\"IsOther\":false},{\"Idx\":1,\"Text\":\"bbb\",\"IsOther\":false},{\"Idx\":2,\"Text\":\"ccc\",\"IsOther\":false}]}]}";
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
