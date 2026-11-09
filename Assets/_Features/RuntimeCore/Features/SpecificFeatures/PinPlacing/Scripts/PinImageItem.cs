using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One image of a pin in the editing context UI: thumbnail (click opens it big), description, order arrows and delete.
/// </summary>
public class PinImageItem : MonoBehaviour {

    public RawImage Thumbnail;
    public Button ThumbnailButton;
    public TMP_InputField DescriptionInput;
    public GameObject LeftButton;
    public GameObject RightButton;

    int _index;
    Action<int, int> _onMove;
    Action<int> _onDelete;
    Action<int, string> _onDescriptionChanged;
    Action<int> _onOpen;

    void Awake() {
        if (DescriptionInput != null) DescriptionInput.onValueChanged.AddListener(value => _onDescriptionChanged?.Invoke(_index, value));
        if (ThumbnailButton != null) ThumbnailButton.onClick.AddListener(() => _onOpen?.Invoke(_index));
    }

    public void Initialize(Texture texture, int index, int count, string description,
        Action<int, int> onMove, Action<int> onDelete, Action<int, string> onDescriptionChanged, Action<int> onOpen) {
        _index = index;
        _onMove = onMove;
        _onDelete = onDelete;
        _onDescriptionChanged = onDescriptionChanged;
        _onOpen = onOpen;

        Thumbnail.texture = texture;
        Thumbnail.uvRect = CenterCrop(texture);
        SetDescriptionWithoutNotify(description);

        // The first image can't move left and the last can't move right
        LeftButton.SetActive(index > 0);
        RightButton.SetActive(index < count - 1);
    }

    public void SetDescriptionWithoutNotify(string description) {
        if (DescriptionInput != null) DescriptionInput.SetTextWithoutNotify(description ?? "");
    }

    public void OnLeft() => _onMove?.Invoke(_index, -1);
    public void OnRight() => _onMove?.Invoke(_index, 1);
    public void OnDelete() => _onDelete?.Invoke(_index);

    /// <summary>The square middle part of the texture, so thumbnails fill their square without stretching.</summary>
    public static Rect CenterCrop(Texture texture) {
        if (texture == null || texture.height == 0) return new Rect(0, 0, 1, 1);
        float aspect = (float)texture.width / texture.height;
        return aspect >= 1f
            ? new Rect((1f - 1f / aspect) * 0.5f, 0, 1f / aspect, 1)
            : new Rect(0, (1f - aspect) * 0.5f, 1, aspect);
    }
}
