using UnityEngine;
using SurveySystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class SurveyTester : MonoBehaviour, IInitializationListener {

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

        StartCoroutine(WaitForInit());
    }

    void LoadFromJson() {
        string json = "{\"Name\":\"\",\"Questions\":[{\"Id\":0,\"Title\":\"nazev\",\"Description\":\"popis\",\"ViewPointId\":\"4f57e61b-76a1-4509-8e69-9b7c00d7100d\",\"QuestionType\":0,\"Answers\":[{\"Idx\":0,\"Text\":\"fdsf\",\"IsOther\":false},{\"Idx\":1,\"Text\":\"asdasd\",\"IsOther\":false},{\"Idx\":2,\"Text\":\"fdsf\",\"IsOther\":false},{\"Idx\":3,\"Text\":\"\",\"IsOther\":true}]},{\"Id\":1,\"Title\":\"\",\"Description\":\"\",\"ViewPointId\":\"cbe5603c-d79e-47f8-8211-b4582625021f\",\"QuestionType\":0,\"Answers\":[{\"Idx\":0,\"Text\":\"\",\"IsOther\":false}]}]}";
        string kson = "";
        SurveyManager.Instance.DeserializeEditor(json);
    }

    // Waiting for survey manager to instantiate his stuff
    // In scenes used in the app this wait wont be needed
    IEnumerator WaitForInit() {
        yield return new WaitForSeconds(0.2f);
        SurveyManager.Instance.EnterSurveyBuilding();
   //     LoadFromJson();
    }


}
