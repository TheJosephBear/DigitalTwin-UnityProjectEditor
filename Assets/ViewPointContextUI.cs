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
        MainManagerBase.Instance.ChangeState(AppState.Freecam);
        MainManagerBase.Instance.ViewManager.ExitViewMoving();
    }

    public void OnDelete() {
        MainManagerBase.Instance.ViewManager.DeleteViewPoint(_vp);
        OnFinished();
    }
}
