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

    [Header("Image popup")]
    [Tooltip("Opened by clicking a thumbnail: the image big with its description")]
    public GameObject ImagePopup;
    public RawImage PopupImage;
    public AspectRatioFitter PopupFitter;
    public GameObject PopupArrows;
    public TextMeshProUGUI PopupCounter;
    public TMP_InputField PopupDescriptionInput;

    PinManager _pinManager;
    PinObject _pin;
    string _originalName;
    string _originalDescription;
    Vector3 _originalPos;
    Vector3 _originalNormal;
    List<string> _originalImageIds;
    List<string> _originalImageDescriptions;
    readonly List<PinImageItem> _imageItems = new List<PinImageItem>();
    int _popupIndex = -1;
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
        _originalImageDescriptions = new List<string>(pin.ImageDescriptions);

        NameInput.SetTextWithoutNotify(pin.Name);
        DescriptionInput.SetTextWithoutNotify(pin.Description);
        if (IconImage != null) {
            IconImage.sprite = pin.Image;
            IconImage.gameObject.SetActive(pin.Image != null);
        }
        CloseImagePopup();
        RefreshImages();

        ToggleUnsavedChanges(false);
    }

    void Awake() {
        if (PopupDescriptionInput != null) PopupDescriptionInput.onValueChanged.AddListener(value => SetImageDescription(_popupIndex, value));
    }

    void OnDisable() {
        CloseImagePopup();
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
                _pin.ImageDescriptions.Add("");
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

        List<string> descriptions = _pin.ImageDescriptions;
        (ids[index], ids[newIndex]) = (ids[newIndex], ids[index]);
        (descriptions[index], descriptions[newIndex]) = (descriptions[newIndex], descriptions[index]);
        RefreshImages();
        ToggleUnsavedChanges(true);
    }

    // Only removed from the pin, the image stays in the project like other unused images
    void DeleteImage(int index) {
        _pin.ImageIds.RemoveAt(index);
        _pin.ImageDescriptions.RemoveAt(index);
        RefreshImages();
        ToggleUnsavedChanges(true);
    }

    void RefreshImages() {
        if (ImageListContent == null || ImageItemPrefab == null) return;

        foreach (Transform child in ImageListContent) {
            if (AddImageButton == null || child.gameObject != AddImageButton) Destroy(child.gameObject);
        }
        _imageItems.Clear();

        List<string> ids = _pin.ImageIds;
        for (int i = 0; i < ids.Count; i++) {
            TextureAsset asset = ImageManager.Instance != null ? ImageManager.Instance.GetTextureAssetByID(ids[i]) : null;
            PinImageItem item = Instantiate(ImageItemPrefab, ImageListContent).GetComponent<PinImageItem>();
            item.Initialize(asset != null ? asset.Texture : null, i, ids.Count, _pin.ImageDescriptions[i], MoveImage, DeleteImage, SetImageDescription, OpenImagePopup);
            _imageItems.Add(item);
        }

        if (AddImageButton != null) AddImageButton.transform.SetAsFirstSibling();
    }

    // From the tile's field or the popup's, the other one is updated to match
    void SetImageDescription(int index, string value) {
        if (index < 0 || index >= _pin.ImageDescriptions.Count || _pin.ImageDescriptions[index] == value) return;

        _pin.ImageDescriptions[index] = value;
        if (index < _imageItems.Count) _imageItems[index].SetDescriptionWithoutNotify(value);
        if (index == _popupIndex && PopupDescriptionInput != null) PopupDescriptionInput.SetTextWithoutNotify(value);
        ToggleUnsavedChanges(true);
    }

    void OpenImagePopup(int index) {
        if (ImagePopup == null || index < 0 || index >= _pin.ImageIds.Count) return;

        _popupIndex = index;
        TextureAsset asset = ImageManager.Instance != null ? ImageManager.Instance.GetTextureAssetByID(_pin.ImageIds[index]) : null;
        Texture texture = asset != null ? asset.Texture : null;
        int count = _pin.ImageIds.Count;

        PopupImage.texture = texture;
        if (PopupFitter != null) PopupFitter.aspectRatio = texture != null && texture.height > 0 ? (float)texture.width / texture.height : 1f;
        if (PopupArrows != null) PopupArrows.SetActive(count > 1);
        if (PopupCounter != null) {
            PopupCounter.text = $"{index + 1} / {count}";
            PopupCounter.transform.parent.gameObject.SetActive(count > 1);
        }
        PopupDescriptionInput.SetTextWithoutNotify(_pin.ImageDescriptions[index]);
        ImagePopup.SetActive(true);
    }

    /// <summary>Hooks to the popup's arrows.</summary>
    public void OnPopupPrevious() => StepPopup(-1);
    public void OnPopupNext() => StepPopup(1);

    void StepPopup(int direction) {
        int count = _pin.ImageIds.Count;
        if (_popupIndex < 0 || count == 0) return;
        OpenImagePopup((_popupIndex + direction + count) % count);
    }

    /// <summary>Hook to the popup's close button and its dark background.</summary>
    public void CloseImagePopup() {
        _popupIndex = -1;
        if (ImagePopup != null) ImagePopup.SetActive(false);
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
        _pin.SetImages(_originalImageIds, _originalImageDescriptions);
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

        // Same three-way choice as leaving the survey: save and exit / exit without saving / stay
        PopUp.Instance.SaveBeforeExitPopUp((choice) => {
            if (choice == UnsavedChangesChoice.Cancel) return;
            if (choice == UnsavedChangesChoice.SaveAndExit) {
                OnSave();
                return;
            }
            Revert();
            _pinManager.CloseContextUI();
        });
    }
}
