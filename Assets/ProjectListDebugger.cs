using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectListDebugger : MonoBehaviour {
#if UNITY_EDITOR
    public bool DebugMode = false;

    void Start() {
        if(DebugMode) StartCoroutine(LoadCouroutine());
    }

    IEnumerator LoadCouroutine() {
        var load = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        yield return new WaitUntil(() => load.isDone);
        FindAnyObjectByType<ProjectListUINew>().Initialize();
    }
#endif
}
