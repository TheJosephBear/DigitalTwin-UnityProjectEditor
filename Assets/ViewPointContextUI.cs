using TMPro;
using UnityEngine;

public class ViewPointContextUI : MonoBehaviour {

    public TMP_InputField NameTextField;
    ViewPoint _vp;

    public void Initialize(ViewPoint vp) {
        _vp = vp;
        NameTextField.text = vp.Name;
    }

    public void OnNameTextFieldChanged(string value) {
        _vp.SetName(value);
    }

    public void OnTextFieldFocused() {
        MainManagerBase.Instance.ViewManager.ToggleMovementScript(false);
    }

    public void OnTextFieldUnFocused() {
        MainManagerBase.Instance.ViewManager.ToggleMovementScript(true);
    }

    public void OnFinished() {
     //   MainManagerBase.Instance.ViewManager.ExitViewMoving();
    }

    public void OnSave() {
        MainManagerBase.Instance.ViewManager.ExitViewMoving(save: true);
    }

    public void OnDelete() {
        MainManagerBase.Instance.ViewManager.DeleteViewPoint(_vp);
        MainManagerBase.Instance.ViewManager.ExitViewMoving(save: true);
    }

    public void OnCancel() {
        MainManagerBase.Instance.ViewManager.ExitViewMoving(save: false);
    }
}
