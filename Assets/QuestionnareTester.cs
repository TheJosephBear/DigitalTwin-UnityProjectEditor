using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuestionnareTester : MonoBehaviour {

    void Awake() {
        StartCoroutine(InitializeCoroutine());
    }

    IEnumerator InitializeCoroutine() {
        AsyncOperation loading = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        while (!loading.isDone) {
            yield return null;
        }
        

        ViewManager.Instance.CreateNewViewPoint();
        ViewManager.Instance.CreateNewViewPoint();
        ViewManager.Instance.CreateNewViewPoint();
        ViewManager.Instance.CreateNewViewPoint();

        SurveyManager.Instance.EnterSurveyBuilding();
    }
}
