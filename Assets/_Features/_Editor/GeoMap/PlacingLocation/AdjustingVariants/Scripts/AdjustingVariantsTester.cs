using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdjustingVariantsTester : MonoBehaviour {
#if UNITY_EDITOR

    private void Awake() {
        StartCoroutine(LoadCouroutine());
    }

    IEnumerator LoadCouroutine() {
        var load = SceneManager.LoadSceneAsync("Utilities", LoadSceneMode.Additive);
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(0.2f);
        // Set up some base map that has some localization based rotation and position
        SceneLoadingManager.Instance.SetActiveScene(SceneType.AdjustingVariantsScene);
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMap);
    }
    void OnFileSelectedMap(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.UploadBaseMapModel(AssetManager.Instance.CreateNewAsset(files[0]));
            EditorManager.Instance.MapManager.SetBaseMapPositionAndRotation(new Vector3(1, 1, 1), Quaternion.Euler(0, 45, 0));
            // Ask for variant
            FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedVar);
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }
    void OnFileSelectedVar(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            EditorManager.Instance.MapManager.UploadMapVariant(AssetManager.Instance.CreateNewAsset(files[0]));
            MapVariantAdjustManager.Instance.EnterAdjusting();
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }

#endif
}