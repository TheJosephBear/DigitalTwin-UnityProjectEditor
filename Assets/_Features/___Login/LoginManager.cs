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
        ServerCommunicationManager.Instance.Login(username, password, (successBool, message) => {
            if (successBool) {
                // Unload login scene and load project list
                StartCoroutine(GoToProjectList());
                UIManager.Instance.HideUI(UIType.Login);
            } else {
                PopUp.Instance.ShowPopUpWindow("Jméno nebo heslo není správně.");
            }
        });
    }

    public void Register(string username, string password) {
        ServerCommunicationManager.Instance.Register(username, password, (successBool, message) => {
            if (successBool) {
                UIManager.Instance.ShowUI(UIType.Login);
                UIManager.Instance.HideUI(UIType.Register);
                PopUp.Instance.ShowPopUpWindow("Registrace proběhla úspěšně.");
            } else {
                PopUp.Instance.ShowPopUpWindow("Registrace selhala.");
            }
        });
    }

    IEnumerator GoToProjectList() {
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(projectListScene, 0f);
        yield return new WaitUntil(() => loadTask.IsCompleted);
        if (loadTask.Result) {
            UIManager.Instance.HideUI(UIType.Login);
            var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Login); // No need to wait for this
        } else {
            Debug.LogError("Failed to load project list scene.");
        }
    }

}
