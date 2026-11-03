using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PinContextUI : MonoBehaviour {

    public TMP_InputField NameInput;
    public TMP_InputField DescriptionInput;
    public Image IconImage;
    public GameObject SaveButtonRef;

    [Header("Images")]
    public RectTransform ImageListContent;
    public GameObject ImageItemPrefab;
    [Tooltip("First tile of the image list (always in view, the list scrolls), opens the image picker")]
    public GameObject AddImageButton;

    PinManager _pinManager;
    PinObject _pin;
    string _originalName;
    string _originalDescription;
    Vector3 _originalPos;
    Vector3 _originalNormal;
    List<string> _originalImageIds;
    bool _unsavedChanges = false;

    public bool HasUnsavedChanges => _unsavedChanges;

    public void Initialize(PinObject pin, PinManager pinManager) {
        _pin = pin;
        _pinManager = pinManager;

        _originalName = pin.Name;
        _originalDescription = pin.Description;
        _originalPos = pin.transform.position;
        _originalNormal = pin.SurfaceNormal;
        _originalImageIds = new List<string>(pin.ImageIds);

        NameInput.SetTextWithoutNotify(pin.Name);
        DescriptionInput.SetTextWithoutNotify(pin.Description);
        if (IconImage != null) {
            IconImage.sprite = pin.Image;
            IconImage.gameObject.SetActive(pin.Image != null);
        }
        RefreshImages();

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

    #region Images

    /// <summary>Hook to the add image tile. Several images can be picked at once.</summary>
    public void OnAddImages() {
        PinObject pin = _pin;
        ImageManager.Instance.AskForImagesDialog(assets => {
            if (pin != _pin) return; // another pin was opened while the dialog was up

            bool added = false;
            foreach (TextureAsset asset in assets) {
                if (_pin.ImageIds.Contains(asset.ID)) continue;
                _pin.ImageIds.Add(asset.ID);
                added = true;
            }
            if (!added) return;

            RefreshImages();
            ToggleUnsavedChanges(true);
        });
    }

    void MoveImage(int index, int direction) {
        int newIndex = index + direction;
        List<string> ids = _pin.ImageIds;
        if (newIndex < 0 || newIndex >= ids.Count) return;

        (ids[index], ids[newIndex]) = (ids[newIndex], ids[index]);
        RefreshImages();
        ToggleUnsavedChanges(true);
    }

    // Only removed from the pin, the image stays in the project like other unused images
    void DeleteImage(int index) {
        _pin.ImageIds.RemoveAt(index);
        RefreshImages();
        ToggleUnsavedChanges(true);
    }

    void RefreshImages() {
        if (ImageListContent == null || ImageItemPrefab == null) return;

        foreach (Transform child in ImageListContent) {
            if (AddImageButton == null || child.gameObject != AddImageButton) Destroy(child.gameObject);
        }

        List<string> ids = _pin.ImageIds;
        for (int i = 0; i < ids.Count; i++) {
            TextureAsset asset = ImageManager.Instance != null ? ImageManager.Instance.GetTextureAssetByID(ids[i]) : null;
            PinImageItem item = Instantiate(ImageItemPrefab, ImageListContent).GetComponent<PinImageItem>();
            item.Initialize(asset != null ? asset.Texture : null, i, ids.Count, MoveImage, DeleteImage);
        }

        if (AddImageButton != null) AddImageButton.transform.SetAsFirstSibling();
    }

    #endregion

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
        _pin.SurfaceNormal = _originalNormal;
        _pin.SetImageIds(_originalImageIds);
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
