using UnityEngine;
using UnityEngine.UI;

// Colors the icon on this object (Image, RawImage or SpriteRenderer) from the IconTheme asset.
// Put it on every uGUI icon; pick OnDark for icons on dark or colored backgrounds.
// Scripts that dim or hide an icon should set Opacity rather than the graphic's color.
[ExecuteAlways]
[DisallowMultipleComponent]
public class ThemedIcon : MonoBehaviour {
    [SerializeField] private IconTheme.Role role = IconTheme.Role.Icon;
    [Range(0f, 1f)]
    [SerializeField] private float opacity = 1f;

    public IconTheme.Role Role {
        get => role;
        set { role = value; Apply(); }
    }

    public float Opacity {
        get => opacity;
        set { opacity = Mathf.Clamp01(value); Apply(); }
    }

    private void OnEnable() {
        IconTheme.Changed += Apply;
        Apply();
    }

    private void OnDisable() {
        IconTheme.Changed -= Apply;
    }

    private void OnValidate() {
        Apply();
    }

    public void Apply() {
        if (this == null) return;
        IconTheme theme = IconTheme.Current;
        if (theme == null) return;

        Color color = theme.ColorFor(role);
        color.a *= opacity;

        // Only assign on a change, so opening a prefab doesn't mark it modified
        if (TryGetComponent(out Graphic graphic)) {
            if (graphic.color != color) graphic.color = color;
        } else if (TryGetComponent(out SpriteRenderer spriteRenderer)) {
            if (spriteRenderer.color != color) spriteRenderer.color = color;
        }
    }
}
