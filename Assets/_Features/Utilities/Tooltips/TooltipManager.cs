using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipManager: MonoBehaviour {
    public static TooltipManager Instance;

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private Vector2 offset = new Vector2(15f, -15f);
    [SerializeField] private float showDelay = 0.35f;

    private Canvas canvas;
    private RectTransform tooltipRect;
    private Coroutine showTooltipCoroutine;
    private readonly Vector3[] worldCorners = new Vector3[4];

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        canvas = GetComponentInParent<Canvas>();
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        HideTooltip();
    }

    private void Update() {
        if (tooltipPanel.activeSelf) {
            // Follow the mouse cursor position
            Vector2 mousePos = Input.mousePosition;
            tooltipPanel.transform.position = ClampToScreen(mousePos + offset);
        }
    }

    private Vector2 ClampToScreen(Vector2 desiredPosition) {
        if (tooltipRect == null)
            return desiredPosition;

        tooltipPanel.transform.position = desiredPosition;
        tooltipRect.GetWorldCorners(worldCorners);

        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        float left = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[0]).x;
        float bottom = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[0]).y;
        float right = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[2]).x;
        float top = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[2]).y;

        if (right > Screen.width)
            desiredPosition.x -= right - Screen.width;
        if (left < 0f)
            desiredPosition.x -= left;
        if (top > Screen.height)
            desiredPosition.y -= top - Screen.height;
        if (bottom < 0f)
            desiredPosition.y -= bottom;

        return desiredPosition;
    }

    public void ShowTooltip(string text) {
        if (showTooltipCoroutine != null)
            StopCoroutine(showTooltipCoroutine);

        showTooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay(text));
    }

    public void HideTooltip() {
        if (showTooltipCoroutine != null) {
            StopCoroutine(showTooltipCoroutine);
            showTooltipCoroutine = null;
        }

        tooltipPanel.SetActive(false);
    }

    private System.Collections.IEnumerator ShowTooltipAfterDelay(string text) {
        if (showDelay > 0f)
            yield return new WaitForSecondsRealtime(showDelay);

        tooltipText.text = text;
        tooltipPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        Vector2 mousePos = Input.mousePosition;
        tooltipPanel.transform.position = ClampToScreen(mousePos + offset);
        showTooltipCoroutine = null;
    }
}
