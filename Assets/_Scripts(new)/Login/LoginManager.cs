using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginManager : Singleton<LoginManager> {

    protected override void Awake() {
        base.Awake();
    }

    public void Login(string username, string password) {
        if(TryLogin(username, password)) {
            UImanager.Instance.ShowUI(UIType.Projects);
            UImanager.Instance.HideUI(UIType.Login);
        } else {
            PopUp.Instance.ShowPopUpWindow("Jméno nebo heslo není správnì.");
        }
    }

    public void Register(string username, string password) {
        if (TryRegister(username, password)) {
            UImanager.Instance.ShowUI(UIType.Login);
            UImanager.Instance.HideUI(UIType.Register);
            PopUp.Instance.ShowPopUpWindow("Registrace probìhla úspìšnì.");
        } else {
            PopUp.Instance.ShowPopUpWindow("Registrace selhala.");
        }
    }

    bool TryLogin(string username, string password) {
        bool success = true;
    /*    WebCommunicationManager.Instance.Login(username, password, (successBool, message) => {
            success = successBool;
            if (message!=null) print(message);
        });*/
        return success;
    }

    bool TryRegister(string username, string password) {
        bool success = true;
    /*    WebCommunicationManager.Instance.Register(username, password, (successBool, message) => {
            success = successBool;
            if (message != null) print(message);
        });*/
        return success;
    }
}
