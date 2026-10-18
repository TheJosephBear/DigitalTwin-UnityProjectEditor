using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PinTesting: MonoBehaviour {

    // Kept inactive in the scene: PinManager creates its UI on enable, which needs the Utilities scene loaded
    public GameObject PinManagerObject;

    void Start() {
        StartCoroutine(LoadCouroutine());
    }

    IEnumerator LoadCouroutine() {
        var load = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(0.2f);
        PinManagerObject.SetActive(true);
    }
}
