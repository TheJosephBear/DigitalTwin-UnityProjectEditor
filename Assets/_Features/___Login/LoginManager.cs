using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SimpleFileBrowser.FileBrowser;

public class LoginManager : Singleton<LoginManager> {

    public SceneType projectListScene;

    protected override void Awake() {
        base.Awake();
    }

    public void Login(string username, string password) {
        WebCommunicationManager.Instance.Login(username, password, (successBool, message) => {
            if (successBool) {
                // Unload login scene and load project list
                StartCoroutine(GoToProjectList());
                UImanager.Instance.HideUI(UIType.Login);
            } else {
                PopUp.Instance.ShowPopUpWindow("Jméno nebo heslo není správnì.");
            }
        });
    }

    public void Register(string username, string password) {
        WebCommunicationManager.Instance.Register(username, password, (successBool, message) => {
            if (successBool) {
                UImanager.Instance.ShowUI(UIType.Login);
                UImanager.Instance.HideUI(UIType.Register);
                PopUp.Instance.ShowPopUpWindow("Registrace probìhla úspìšnì.");
            } else {
                PopUp.Instance.ShowPopUpWindow("Registrace selhala.");
            }
        });
    }

    IEnumerator GoToProjectList() {
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(projectListScene, 0f);
        yield return new WaitUntil(() => loadTask.IsCompleted);
        if (loadTask.Result) {
            UImanager.Instance.HideUI(UIType.Login);
            var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Login); // No need to wait for this
        } else {
            Debug.LogError("Failed to load project list scene.");
        }
    }

}
