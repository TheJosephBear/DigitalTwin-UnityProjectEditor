using UnityEngine;
using SurveySystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class SurveyTester : MonoBehaviour {

    SurveySystem.SurveyBuilder _builder;
    ViewManager _viewManager;

    private void Start() {
        _builder = FindAnyObjectByType<SurveySystem.SurveyBuilder>();
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


        _builder.CreateNewSurvey();
    }

    IEnumerator LoadUitlitiesAndStartTest() {
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }

        Test();
    }


}
