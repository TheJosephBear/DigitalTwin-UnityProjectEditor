using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Read only pin context UI for the viewer: shows what was written for the pin in the editor.
/// Images are shown one at a time between the title and the description, clicking one opens it big.
/// </summary>
public class PinInfoUI : MonoBehaviour {

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public Image IconImage;

    [Header("Gallery (in the panel)")]
    public GameObject Gallery;
    public RawImage GalleryImage;
    public AspectRatioFitter GalleryFitter;
    public GameObject GalleryArrows;
    public TextMeshProUGUI GalleryCounter;

    [Header("Enlarged image")]
    public GameObject Lightbox;
    public RawImage LightboxImage;
    public AspectRatioFitter LightboxFitter;
    public GameObject LightboxArrows;
    public TextMeshProUGUI LightboxCounter;

    PinManager _pinManager;
    readonly List<Texture> _images = new List<Texture>();
    int _index;

    public void Initialize(PinObject pin, PinManager pinManager) {
        _pinManager = pinManager;

        NameText.text = pin.Name;
        DescriptionText.text = pin.Description;
        DescriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(pin.Description));

        if (IconImage != null) {
            IconImage.sprite = pin.Image;
            IconImage.gameObject.SetActive(pin.Image != null);
        }

        // Images that failed to download are left out
        _images.Clear();
        foreach (string id in pin.ImageIds) {
            TextureAsset asset = ImageManager.Instance != null ? ImageManager.Instance.GetTextureAssetByID(id) : null;
            if (asset != null && asset.Texture != null) _images.Add(asset.Texture);
        }
        _index = 0;

        Gallery.SetActive(_images.Count > 0);
        CloseLightbox();
        ShowImage();
    }

    void OnDisable() {
        CloseLightbox();
    }

    /// <summary>Hook to the X button.</summary>
    public void OnX() {
        _pinManager.CloseContextUI();
    }

    #region Images

    // Both views always show the same image, so the arrows in either one move both
    public void OnPreviousImage() => StepImage(-1);
    public void OnNextImage() => StepImage(1);

    void StepImage(int direction) {
        if (_images.Count == 0) return;
        _index = (_index + direction + _images.Count) % _images.Count;
        ShowImage();
    }

    void ShowImage() {
        if (_images.Count == 0) return;

        Texture texture = _images[_index];
        float aspect = texture.height > 0 ? (float)texture.width / texture.height : 1f;
        string counter = $"{_index + 1} / {_images.Count}";
        bool several = _images.Count > 1;

        GalleryImage.texture = texture;
        GalleryFitter.aspectRatio = aspect;
        GalleryArrows.SetActive(several);
        GalleryCounter.text = counter;
        GalleryCounter.transform.parent.gameObject.SetActive(several);

        LightboxImage.texture = texture;
        LightboxFitter.aspectRatio = aspect;
        LightboxArrows.SetActive(several);
        LightboxCounter.text = counter;
        LightboxCounter.transform.parent.gameObject.SetActive(several);
    }

    /// <summary>Hook to clicking the image in the panel.</summary>
    public void OpenLightbox() {
        if (_images.Count == 0) return;
        Lightbox.SetActive(true);
    }

    /// <summary>Hook to the lightbox's close button and its dark background.</summary>
    public void CloseLightbox() {
        if (Lightbox != null) Lightbox.SetActive(false);
    }

    #endregion
}
