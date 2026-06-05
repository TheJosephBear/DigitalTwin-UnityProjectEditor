using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SimpleFileBrowser.FileBrowser;

public class AuthorizationManager : Singleton<AuthorizationManager> {

    public SceneType projectListScene;
    public AuthorizationUI AuthorizationUI;


    protected override void Awake() {
        base.Awake();
        // Try loggin in with saved session (if exists)
        LoginWithSession(); // Empty credentials will trigger session login attempt on server side
    }

    public void Login(string username, string password) {
        ServerCommunicationManager.Instance.Login(username, password, (successBool, message) => {
            if (successBool) {
                // Unload login scene and load project list
                StartCoroutine(GoToProjectList());
            } else {
                MessageDisplayManager.Instance.ShowMessage("Jméno nebo heslo není správně.");
            }
        });
    }

    public void LoginWithSession() {
        ServerCommunicationManager.Instance.Login("", "", (successBool, message) => {
            if (successBool) {
                Debug.Log("Valid session found, proceeding to project list.");
                // Unload login scene and load project list
                StartCoroutine(GoToProjectList());
            } else {
                Debug.Log("No valid session found, showing login screen.");
            }
        });
    }

    public void Logout() {
        ServerCommunicationManager.Instance.Logout((successBool, message) => {
            if (successBool) {
           //     AuthorizationUI.GoToLogin();
                SceneLoadingManager.Instance.LoadSceneAsync(SceneType.Login, 0f);
                var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.ProjectList); // No need to wait for this
            } else {
                PopUp.Instance.ShowPopUpWindow("Odhlášení selhalo.");
            }
        });
    }

    public void Register(string username, string password) {
        ServerCommunicationManager.Instance.Register(username, password, (successBool, message) => {
            if (successBool) {
                AuthorizationUI.GoToLogin();
                MessageDisplayManager.Instance.ShowMessage("Registrace proběhla úspěšně.");
            } else {
                MessageDisplayManager.Instance.ShowMessage("Registrace selhala.");
            }
        });
    }

    IEnumerator GoToProjectList() {
        var loadTask = SceneLoadingManager.Instance.LoadSceneAsync(projectListScene, 0f);
        yield return new WaitUntil(() => loadTask.IsCompleted);
        if (loadTask.Result) {
            var unloadTask = SceneLoadingManager.Instance.UnLoadSceneAsync(SceneType.Login); // No need to wait for this
        } else {
            Debug.LogError("Failed to load project list scene.");
        }
    }

}
