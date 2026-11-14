using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthorizationUI : UIBehaviour {
    
    public TMP_InputField name_field;
    public TMP_InputField password_field;

    public GameObject LoginElementContainer;
    public GameObject RegisterElementContainer;

    private int _lastSubmitFrame = -1;

    private void Awake() {
        // Enter in the name field goes on to the password, Enter in the password logs in (or registers)
        name_field.onSubmit.AddListener(_ => OnNameSubmitted());
        password_field.onSubmit.AddListener(_ => OnPasswordSubmitted());

        GoToLogin();
    }

    public void GoToLogin() {
        LoginElementContainer.SetActive(true);
        RegisterElementContainer.SetActive(false);
        FocusFirstEmptyField();
    }

    public void GoToRegister() {
        LoginElementContainer.SetActive(false);
        RegisterElementContainer.SetActive(true);
        FocusFirstEmptyField();
    }

    void FocusFirstEmptyField() {
        bool nameFilled = !string.IsNullOrEmpty(name_field.text);
        InputFieldNavigation.Focus(nameFilled ? password_field : name_field);
    }

    void OnNameSubmitted() {
        if (string.IsNullOrEmpty(password_field.text)) {
            InputFieldNavigation.Focus(password_field);
        } else {
            OnPasswordSubmitted();
        }
    }

    void OnPasswordSubmitted() {
        // One Enter can come both from the field and from the UI submit action
        if (_lastSubmitFrame == Time.frameCount) return;
        _lastSubmitFrame = Time.frameCount;

        if (RegisterElementContainer.activeSelf) Register();
        else Login();
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
