using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginRegister : UIBehaviour {

    public TMP_InputField name_field;
    public TMP_InputField password_field;

    public override void Hide() {
        canvas.SetActive(false);
    }

    public override void Show() {
        canvas.SetActive(true);
    }

    public void GoToLogin() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.ShowUI(UIType.Login);
        UImanager.Instance.HideUI(UIType.Register);
    }

    public void GoToRegister() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.ShowUI(UIType.Register);
        UImanager.Instance.HideUI(UIType.Login);
    }

    public void Login() {
        AudioManager.Instance.PlaySound(SoundType.click);
        string name = name_field.text;
        string password = password_field.text;
        LoginManager.Instance.Login(name, password);
    }

    public void Register() {
        AudioManager.Instance.PlaySound(SoundType.click);
        string name = name_field.text;
        string password = password_field.text;
        LoginManager.Instance.Register(name, password);
    }


}
