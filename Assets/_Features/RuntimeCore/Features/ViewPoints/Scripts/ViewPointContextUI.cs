using TMPro;
using UnityEngine;

public class ViewPointContextUI : MonoBehaviour {

    public TMP_InputField NameTextField;
    public GameObject SaveButtonRef;
    ViewMovingManager _viewMovingManager;
    ViewPoint _vp;

    public void Initialize(ViewPoint vp, ViewMovingManager viewMovingScript) {
        _vp = vp;
        NameTextField.text = vp.Name;
        _viewMovingManager = viewMovingScript;
    }

    public void OnNameTextFieldChanged(string value) {
        string oldName = _vp.Name;
        if (oldName == value) return;
        _vp.SetName(value);
        _viewMovingManager.ToggleUnsavedChanges(true);
    }

    public void ToggleSaveButton(bool toggleOn) {
        SaveButtonRef.SetActive(toggleOn);
    }

    public void OnTextFieldFocused() {
        FindAnyObjectByType<ViewMovingManager>().ToggleMovementScript(false);
    }

    public void OnTextFieldUnFocused() {
        FindAnyObjectByType<ViewMovingManager>().ToggleMovementScript(true);
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
        MainManagerBase.Instance.ViewManager.ExitViewMoving(
            save: false,
            message: "Neuložené změny, přejete si odejít?"
        );
    }
}
