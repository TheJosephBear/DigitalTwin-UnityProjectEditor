using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public static class SurveyUIUtils {
    public const string RotateButtonName = "rotate-image";
    private const float RotateButtonGap = 6f;

    // Images that can be turned, so the enlarged view offers the same button
    private static readonly ConditionalWeakTable<VisualElement, Action> RotateActions = new ConditionalWeakTable<VisualElement, Action>();

    // Editor: turns the image by 90° per click, for photos whose orientation wasn't fixed automatically.
    // Added before AddUploadOverlay, so hovering the button hides the upload hint like the other buttons
    public static void AddRotateButton(VisualElement imageElement, Action onRotate, string positionClass = "media-rotate-button") {
        if (imageElement == null || imageElement.Q<Button>(RotateButtonName) != null) return;

        var button = new Button { name = RotateButtonName, tooltip = "Otočit o 90°" };
        button.AddToClassList("icon");
        button.AddToClassList("enhance-button");
        button.AddToClassList("rotate-button");
        button.AddToClassList(positionClass);
        button.RegisterCallback<ClickEvent>(evt => {
            evt.StopPropagation(); // Otherwise the click reaches the image and opens the upload dialog
            onRotate?.Invoke();
            imageElement.MarkDirtyRepaint(); // A texture turned in place is the same background, but with swapped sides
        });

        // Same size as the enlarge button (sized inline in the templates), just left of it
        // (Button.icon would otherwise make it 32 px)
        var enhanceButton = imageElement.Q<Button>("enhance-image");
        float size = enhanceButton != null ? enhanceButton.style.width.value.value : 0f;
        if (size > 0f) {
            button.style.width = size;
            button.style.height = size;
            button.style.bottom = enhanceButton.style.bottom.value.value;
            button.style.right = enhanceButton.style.right.value.value + size + RotateButtonGap;
        }

        imageElement.Add(button);
        RotateActions.AddOrUpdate(imageElement, onRotate);
    }

    // With an even number of points (0–5, 1–10) the middle falls between two values, so the handle of a slider
    // set to the middle value is off centre. center = true shows it exactly in the middle, false where its value is.
    // Call again when the slider's geometry changes
    public static void CenterSliderHandle(SliderInt slider, bool center) {
        if (slider == null) return;
        var dragger = slider.Q(className: "unity-base-slider__dragger");
        var container = slider.Q(className: "unity-base-slider__drag-container");
        if (dragger == null || container == null) return;

        float offset = 0f;
        int range = slider.highValue - slider.lowValue;
        if (center && range > 0) {
            // Same travel as the slider uses for the handle
            float normalized = (float)(slider.value - slider.lowValue) / range;
            float travel = container.layout.width - dragger.resolvedStyle.width
                - dragger.resolvedStyle.marginLeft - dragger.resolvedStyle.marginRight;
            if (!float.IsNaN(travel) && travel > 0f) offset = (0.5f - normalized) * travel;
        }

        // The slider moves the handle with transform.position (the translate style), so the offset goes to left
        if (Mathf.Approximately(offset, 0f)) dragger.style.left = StyleKeyword.Null;
        else dragger.style.left = offset;
    }

    // Editor: on hover the image shows an upload icon and a hand cursor, since clicking it uploads a new image
    public static void AddUploadOverlay(VisualElement imageElement) {
        if (imageElement == null || imageElement.Q("image-upload-overlay") != null) return;

        var overlay = new VisualElement { name = "image-upload-overlay", pickingMode = PickingMode.Ignore };
        overlay.AddToClassList("image-upload-overlay");
        var icon = new VisualElement { pickingMode = PickingMode.Ignore };
        icon.AddToClassList("image-upload-overlay__icon");
        overlay.Add(icon);

        // First child, so the remove and enlarge buttons stay on top
        imageElement.Insert(0, overlay);
        imageElement.AddToClassList("image--uploadable");

        // Over the remove / enlarge buttons a click does what the button says, not an upload, so the overlay hides
        // The image choice option is a Button itself and the query includes it, so it is skipped
        imageElement.Query<Button>().ForEach(button => {
            if (button == imageElement) return;
            button.RegisterCallback<PointerEnterEvent>(_ => imageElement.AddToClassList("image--over-button"));
            button.RegisterCallback<PointerLeaveEvent>(_ => imageElement.RemoveFromClassList("image--over-button"));
        });
    }

    public static void EnhanceImage(VisualElement sourceElement, VisualTreeAsset template = null) {
        if (sourceElement == null) return;

        StyleBackground styleBg = sourceElement.style.backgroundImage;
        Background bg = styleBg.value;
        Background resolvedBg = sourceElement.resolvedStyle.backgroundImage;

        Texture tex = bg.texture ?? resolvedBg.texture;
        RenderTexture rt = bg.renderTexture ?? resolvedBg.renderTexture;
        Sprite sp = bg.sprite ?? resolvedBg.sprite;
        VectorImage vi = bg.vectorImage ?? resolvedBg.vectorImage;

        bool hasImage = tex != null || rt != null || sp != null || vi != null;
        if (!hasImage) {
            Debug.LogWarning($"[EnhanceImage] No image to enhance on element '{sourceElement.name}'!");
            return;
        }

#if UNITY_EDITOR
        if (template == null) {
            template = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Features/RuntimeCore/Features/SpecificFeatures/Survey/UI/FullscreenImageOverlay.uxml");
        }
#endif

        if (template == null) {
            Debug.LogError("FullscreenImageOverlayTemplate is missing!");
            return;
        }

        var root = sourceElement.panel?.visualTree;
        if (root == null) return;

        var existingOverlay = root.Q("fullscreen-overlay-instance");
        if (existingOverlay != null) {
            existingOverlay.RemoveFromHierarchy();
        }

        var overlayInstance = template.Instantiate();
        overlayInstance.name = "fullscreen-overlay-instance";
        overlayInstance.style.position = Position.Absolute;
        overlayInstance.style.top = 0;
        overlayInstance.style.bottom = 0;
        overlayInstance.style.left = 0;
        overlayInstance.style.right = 0;

        var overlay = overlayInstance.Q<VisualElement>("fullscreen-overlay");
        var preview = overlayInstance.Q<VisualElement>("enhanced-image-preview");
        var closeBtn = overlayInstance.Q<Button>("close-overlay-btn");

        if (preview != null) {
            preview.pickingMode = PickingMode.Ignore;
            if (rt != null) {
                preview.style.backgroundImage = Background.FromRenderTexture(rt);
            } else if (tex != null) {
                preview.style.backgroundImage = Background.FromTexture2D((Texture2D)tex);
            } else if (sp != null) {
                preview.style.backgroundImage = Background.FromSprite(sp);
            } else if (vi != null) {
                preview.style.backgroundImage = Background.FromVectorImage(vi);
            } else {
                preview.style.backgroundImage = sourceElement.style.backgroundImage;
            }
        }

        if (closeBtn != null) {
            closeBtn.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation();
                overlayInstance.RemoveFromHierarchy();
            });
        }

        // Turns the source image, the enlarged view then shows what the source shows
        if (overlay != null && preview != null && RotateActions.TryGetValue(sourceElement, out Action rotate)) {
            var rotateBtn = new Button { name = "rotate-overlay-btn", tooltip = "Otočit o 90°" };
            rotateBtn.AddToClassList("fullscreen-rotate-button");
            rotateBtn.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation(); // A click on the dark background closes the view
                rotate?.Invoke();
                preview.style.backgroundImage = sourceElement.style.backgroundImage;
                preview.MarkDirtyRepaint();
                sourceElement.MarkDirtyRepaint();
            });
            overlay.Add(rotateBtn);
        }

        if (overlay != null) {
            overlay.RegisterCallback<ClickEvent>(evt => {
                overlayInstance.RemoveFromHierarchy();
            });
            overlay.RegisterCallback<PointerDownEvent>(evt => {
                if (evt.target == overlay) {
                    overlayInstance.RemoveFromHierarchy();
                    evt.StopPropagation();
                }
            });
        }

        root.Add(overlayInstance);
    }
}
