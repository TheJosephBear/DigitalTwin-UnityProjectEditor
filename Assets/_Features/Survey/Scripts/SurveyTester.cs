using UnityEngine;
using SurveySystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class SurveyTester : MonoBehaviour {

    ViewManager _viewManager;

    private void Start() {
        _viewManager = FindAnyObjectByType<ViewManager>();

        StartCoroutine(LoadUitlitiesAndStartTest());
    }

    void Test() {
        if (_viewManager == null) {
            Debug.LogError("View manager could not be found in scene!");
        }
        _viewManager.CreateNewViewPoint(updateUI: false);
        _viewManager.CreateNewViewPoint(updateUI: false);
        _viewManager.CreateNewViewPoint(updateUI: false);
        _viewManager.CreateNewViewPoint(updateUI: false);


        SurveyManager.Instance.EnterSurveyBuilding();
    }

    IEnumerator LoadUitlitiesAndStartTest() {
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }

        Test();
    }


}
