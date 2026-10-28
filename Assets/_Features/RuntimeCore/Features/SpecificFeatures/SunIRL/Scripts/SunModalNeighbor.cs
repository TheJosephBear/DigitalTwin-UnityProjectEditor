using UnityEngine;

// Keeps a HUD element (e.g. the walk button) just to the right of the Sun modal:
// next to the sun button while the modal is closed, next to the card while it is open.
// It is also scaled to the sun button's on-screen size so the two buttons look like a pair.
// Without a Sun modal in the scene the element stays where it was placed.
// The position is recomputed every frame, so it holds even when the two canvases
// scale differently (e.g. one matches width, the other height) and the window is resized.
[RequireComponent(typeof(RectTransform))]
public class SunModalNeighbor: MonoBehaviour {
    [SerializeField] private float gap = 5f; // in this canvas' units
    [SerializeField] private float slideDuration = 0.2f;
    [SerializeField] private bool matchSunButtonSize = true;

    private readonly Vector3[] corners = new Vector3[4];
    private RectTransform rectTransform;
    private RectTransform anchor;
    private Vector2 slideFrom;
    private float slideTime = -1f; // < 0 when not sliding

    private void Awake() {
        rectTransform = (RectTransform)transform;
    }

    private void OnEnable() {
        SunIRLUI.AnchorChanged += OnAnchorChanged;
        if (SunIRLUI.CurrentAnchor != null) {
            OnAnchorChanged(SunIRLUI.CurrentAnchor, false);
        }
    }

    private void OnDisable() {
        SunIRLUI.AnchorChanged -= OnAnchorChanged;
        slideTime = -1f;
    }

    // After CanvasScaler has updated in Update, before the canvases are rebuilt
    private void LateUpdate() {
        if (anchor == null) return;

        if (matchSunButtonSize && SunIRLUI.SunButton != null) {
            MatchScreenHeight(SunIRLUI.SunButton);
        }

        Vector2 target = PositionNextTo(anchor);
        if (slideTime < 0f) {
            rectTransform.anchoredPosition = target;
            return;
        }

        slideTime += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(slideTime / slideDuration);
        float eased = 1f - (1f - t) * (1f - t); // ease-out
        rectTransform.anchoredPosition = Vector2.LerpUnclamped(slideFrom, target, eased);
        if (t >= 1f) slideTime = -1f;
    }

    private void OnAnchorChanged(RectTransform newAnchor, bool animate) {
        anchor = newAnchor;

        if (animate && slideDuration > 0f) {
            slideFrom = rectTransform.anchoredPosition;
            slideTime = 0f;
        } else {
            rectTransform.anchoredPosition = PositionNextTo(anchor);
            slideTime = -1f;
        }
    }

    // anchoredPosition that puts our bottom-left corner at the anchor's bottom-right corner (+ gap)
    private Vector2 PositionNextTo(RectTransform target) {
        var parent = (RectTransform)rectTransform.parent;

        target.GetWorldCorners(corners);
        Vector2 targetBottomRight = RectTransformUtility.WorldToScreenPoint(CanvasCamera(target), corners[3]);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, targetBottomRight, CanvasCamera(rectTransform), out Vector2 desiredBottomLeft);
        desiredBottomLeft.x += gap;

        rectTransform.GetWorldCorners(corners);
        Vector2 currentBottomLeft = parent.InverseTransformPoint(corners[0]);

        return rectTransform.anchoredPosition + (desiredBottomLeft - currentBottomLeft);
    }

    // Scales us uniformly so our on-screen height equals the reference's
    // (the two canvases may scale differently, so a fixed localScale only matches at one resolution)
    private void MatchScreenHeight(RectTransform reference) {
        float referenceHeight = ScreenHeight(reference);
        float ownHeight = ScreenHeight(rectTransform);
        if (referenceHeight <= 0f || ownHeight <= 0f) return;

        float ratio = referenceHeight / ownHeight;
        if (Mathf.Abs(ratio - 1f) > 0.001f) {
            rectTransform.localScale *= ratio;
        }
    }

    private float ScreenHeight(RectTransform target) {
        target.GetWorldCorners(corners);
        Camera camera = CanvasCamera(target);
        return RectTransformUtility.WorldToScreenPoint(camera, corners[1]).y
             - RectTransformUtility.WorldToScreenPoint(camera, corners[0]).y;
    }

    private static Camera CanvasCamera(Component component) {
        Canvas canvas = component.GetComponentInParent<Canvas>(true);
        if (canvas == null) return null;
        canvas = canvas.rootCanvas;
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }
}
