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
            UImanager.Instance.ShowUI(UIType.Projects);
            UImanager.Instance.HideUI(UIType.Register);
        } else {
            PopUp.Instance.ShowPopUpWindow("Registrace selhala.");
        }
    }

    bool TryLogin(string username, string password) {
        bool success = true;
        /* logic for login */
        return success;
    }

    bool TryRegister(string username, string password) {
        bool success = true;
        /* logic for register */
        return success;
    }




}
