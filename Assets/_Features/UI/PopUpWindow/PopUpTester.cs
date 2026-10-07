using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopUpTester : MonoBehaviour {

    void Start() {
        StartCoroutine(LoadCouroutine());
    }

    IEnumerator LoadCouroutine() {
        var load = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        yield return new WaitUntil(() => load.isDone);
        FindAnyObjectByType<ProjectListUINew>().Initialize();
    }

    public void TestAreYouSure() {

    }


}
