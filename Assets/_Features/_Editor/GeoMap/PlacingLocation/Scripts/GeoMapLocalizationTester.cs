using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeoMapLocalizationTester : MonoBehaviour {

    void Start() {
        StartCoroutine(LoadCouroutine());
    }

    IEnumerator LoadCouroutine() {
        var load = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(0.2f);
        GeoMapManager.Instance.ActivateGeoLocalization();
    }

}
