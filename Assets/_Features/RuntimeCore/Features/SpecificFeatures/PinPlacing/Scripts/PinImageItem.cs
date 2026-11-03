using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One image of a pin in the editing context UI: thumbnail, order arrows and delete.
/// </summary>
public class PinImageItem : MonoBehaviour {

    public RawImage Thumbnail;
    public GameObject LeftButton;
    public GameObject RightButton;

    int _index;
    Action<int, int> _onMove;
    Action<int> _onDelete;

    public void Initialize(Texture texture, int index, int count, Action<int, int> onMove, Action<int> onDelete) {
        _index = index;
        _onMove = onMove;
        _onDelete = onDelete;

        Thumbnail.texture = texture;
        Thumbnail.uvRect = CenterCrop(texture);

        // The first image can't move left and the last can't move right
        LeftButton.SetActive(index > 0);
        RightButton.SetActive(index < count - 1);
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
