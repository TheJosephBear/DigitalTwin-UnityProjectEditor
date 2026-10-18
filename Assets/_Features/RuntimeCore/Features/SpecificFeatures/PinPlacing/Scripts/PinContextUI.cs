using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PinContextUI : MonoBehaviour {

    public TMP_InputField NameInput;
    public TMP_InputField DescriptionInput;
    public Image IconImage;
    public GameObject SaveButtonRef;

    PinManager _pinManager;
    PinObject _pin;
    string _originalName;
    string _originalDescription;
    Vector3 _originalPos;
    bool _unsavedChanges = false;

    public bool HasUnsavedChanges => _unsavedChanges;

    public void Initialize(PinObject pin, PinManager pinManager) {
        _pin = pin;
        _pinManager = pinManager;

        _originalName = pin.Name;
        _originalDescription = pin.Description;
        _originalPos = pin.transform.position;

        NameInput.SetTextWithoutNotify(pin.Name);
        DescriptionInput.SetTextWithoutNotify(pin.Description);
        if (IconImage != null) {
            IconImage.sprite = pin.Image;
            IconImage.gameObject.SetActive(pin.Image != null);
        }

        ToggleUnsavedChanges(false);
    }

    public void OnNameChanged(string value) {
        if (_pin.Name == value) return;
        _pin.SetName(value);
        _pinManager.RefreshPinButtons();
        ToggleUnsavedChanges(true);
    }

    public void OnDescriptionChanged(string value) {
        if (_pin.Description == value) return;
        _pin.SetDescription(value);
        ToggleUnsavedChanges(true);
    }

    public void OnReturnFromMoving() {
        if (_pin.transform.position != _originalPos) ToggleUnsavedChanges(true);
    }

    public void ToggleUnsavedChanges(bool toggleOn) {
        _unsavedChanges = toggleOn;
        SaveButtonRef.GetComponent<Button>().interactable = (_unsavedChanges);
    }

    /// <summary>
    /// Puts the pin back to how it was when the context UI was opened.
    /// </summary>
    public void Revert() {
        _pin.SetName(_originalName);
        _pin.SetDescription(_originalDescription);
        _pin.transform.position = _originalPos;
        _pinManager.RefreshPinButtons();
        ToggleUnsavedChanges(false);
    }

    public void OnSave() {
        if (_unsavedChanges) _pinManager.MarkUnsavedChanges();
        ToggleUnsavedChanges(false);
        _pinManager.CloseContextUI();
    }

    public void OnMove() {
        _pinManager.StartMovingPin(_pin);
    }

    public void OnDelete() {
        _pinManager.DeletePin(_pin);
    }

    public void OnCancel() {
        if (!_unsavedChanges) {
            _pinManager.CloseContextUI();
            return;
        }

        PopUp.Instance.AreYouSurePopUp((sure) => {
            if (!sure) return;
            Revert();
            _pinManager.CloseContextUI();
        }, "Neuložené změny, přejete si odejít?");
    }
}
