using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SimpleFileBrowser.FileBrowser;

public class LoginManager : Singleton<LoginManager> {

    protected override void Awake() {
        base.Awake();
    }

    public void Login(string username, string password) {
        WebCommunicationManager.Instance.Login(username, password, (successBool, message) => {
            if (successBool) {
                UImanager.Instance.ShowUI(UIType.Projects);
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

}
