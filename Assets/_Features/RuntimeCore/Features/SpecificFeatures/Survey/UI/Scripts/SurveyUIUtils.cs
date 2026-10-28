using UnityEngine;
using UnityEngine.UIElements;

public static class SurveyUIUtils {
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
