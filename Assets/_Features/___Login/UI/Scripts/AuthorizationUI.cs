using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthorizationUI : UIBehaviour {
    
    public TMP_InputField name_field;
    public TMP_InputField password_field;

    public GameObject LoginElementContainer;
    public GameObject RegisterElementContainer;

    private void Awake() {
        GoToLogin();
    }

    public void GoToLogin() {
        LoginElementContainer.SetActive(true);
        RegisterElementContainer.SetActive(false);
    }

    public void GoToRegister() {
        LoginElementContainer.SetActive(false);
        RegisterElementContainer.SetActive(true);
    }

    public void Login() {
        string name = name_field.text;
        string password = password_field.text;
        AuthorizationManager.Instance.Login(name, password);
    }

    public void Register() {
        string name = name_field.text;
        string password = password_field.text;
        AuthorizationManager.Instance.Register(name, password);
    }

}
