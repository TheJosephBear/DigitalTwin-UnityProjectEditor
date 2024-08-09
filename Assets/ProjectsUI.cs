using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProjectsUI : UIBehaviour {

    public GameObject canvas;

    public override void Hide() {
        canvas.SetActive(false);
    }

    public override void Show() {
        canvas.SetActive(true);
    }

    public void GoEditing() {
        AudioManager.Instance.PlaySound(SoundType.click);
        StartCoroutine(LoadEditing());
    }

    IEnumerator LoadEditing() {
        var loading = SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Editing, 0f);
        while (!loading.IsCompleted) {
            yield return null;
        }
        UImanager.Instance.HideUI(UIType.Projects);
    }

}
