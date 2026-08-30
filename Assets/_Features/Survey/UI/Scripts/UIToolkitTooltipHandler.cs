using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Runtime tooltip handler for UI Toolkit (UIDocument).
/// Automatically displays tooltips for any VisualElement with a non-empty `tooltip` property.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIToolkitTooltipHandler : MonoBehaviour {

    private UIDocument _uiDocument;
    private VisualElement _root;
    private Label _tooltipLabel;

    private Coroutine _showTooltipCoroutine;
    private VisualElement _currentHoveredElement;
    private string _currentTooltipText;

    [SerializeField]
    private float _tooltipDelay = 0.4f;

    private void Awake() {
        _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable() {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        _root = _uiDocument.rootVisualElement;
        if (_root == null) return;

        SetupTooltipLabel();

        // Use global TrickleDown callbacks on root to catch tooltips on ANY element (including dynamic elements)
        _root.RegisterCallback<PointerOverEvent>(OnPointerOver, TrickleDown.TrickleDown);
        _root.RegisterCallback<PointerOutEvent>(OnPointerOut, TrickleDown.TrickleDown);
        _root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
    }

    private void OnDisable() {
        HideTooltip();

        if (_root != null) {
            _root.UnregisterCallback<PointerOverEvent>(OnPointerOver, TrickleDown.TrickleDown);
            _root.UnregisterCallback<PointerOutEvent>(OnPointerOut, TrickleDown.TrickleDown);
            _root.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        }
    }

    private void SetupTooltipLabel() {
        _tooltipLabel = _root.Q<Label>("tooltipLabel");

        if (_tooltipLabel == null) {
            _tooltipLabel = new Label();
            _tooltipLabel.name = "tooltipLabel";
            _tooltipLabel.AddToClassList("custom-tooltip-label");
            _root.Add(_tooltipLabel);
        }

        _tooltipLabel.pickingMode = PickingMode.Ignore;
        _tooltipLabel.style.position = Position.Absolute;
        _tooltipLabel.style.display = DisplayStyle.None;
    }

    private void OnPointerOver(PointerOverEvent evt) {
        VisualElement target = FindElementWithTooltip(evt.target as VisualElement);

        if (target != null && !string.IsNullOrEmpty(target.tooltip)) {
            if (_currentHoveredElement == target) return;

            _currentHoveredElement = target;
            _currentTooltipText = target.tooltip;

            if (_showTooltipCoroutine != null) {
                StopCoroutine(_showTooltipCoroutine);
            }

            _showTooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay(_tooltipDelay, evt.position));
        }
    }

    private void OnPointerOut(PointerOutEvent evt) {
        VisualElement target = evt.target as VisualElement;
        if (_currentHoveredElement != null && (target == _currentHoveredElement || _currentHoveredElement.Contains(target))) {
            HideTooltip();
        }
    }

    private void OnPointerDown(PointerDownEvent evt) {
        HideTooltip();
    }

    private VisualElement FindElementWithTooltip(VisualElement element) {
        VisualElement curr = element;
        while (curr != null && curr != _root) {
            if (!string.IsNullOrEmpty(curr.tooltip)) {
                return curr;
            }
            curr = curr.parent;
        }
        return null;
    }

    private IEnumerator ShowTooltipAfterDelay(float delay, Vector2 pointerPos) {
        yield return new WaitForSeconds(delay);

        if (_currentHoveredElement == null || string.IsNullOrEmpty(_currentTooltipText) || _tooltipLabel == null) {
            yield break;
        }

        _tooltipLabel.text = _currentTooltipText;
        _tooltipLabel.style.display = DisplayStyle.Flex;
        _tooltipLabel.BringToFront();

        // Calculate layout coordinates
        _tooltipLabel.schedule.Execute(() => {
            if (_tooltipLabel == null || _currentHoveredElement == null || _root == null) return;

            float rootWidth = _root.layout.width;
            float rootHeight = _root.layout.height;

            if (rootWidth <= 0 || rootHeight <= 0) return;

            Vector2 pointerLocal = _root.WorldToLocal(pointerPos);
            float tooltipWidth = _tooltipLabel.layout.width > 0 ? _tooltipLabel.layout.width : 150f;
            float tooltipHeight = _tooltipLabel.layout.height > 0 ? _tooltipLabel.layout.height : 28f;

            // Position slightly below and right of cursor
            float targetLeft = pointerLocal.x + 12f;
            float targetTop = pointerLocal.y + 16f;

            // Boundary checks
            if (targetLeft + tooltipWidth > rootWidth - 10f) {
                targetLeft = pointerLocal.x - tooltipWidth - 8f;
            }
            if (targetTop + tooltipHeight > rootHeight - 10f) {
                targetTop = pointerLocal.y - tooltipHeight - 8f;
            }

            _tooltipLabel.style.left = Mathf.Max(6f, targetLeft);
            _tooltipLabel.style.top = Mathf.Max(6f, targetTop);
        });
    }

    public void HideTooltip() {
        if (_showTooltipCoroutine != null) {
            StopCoroutine(_showTooltipCoroutine);
            _showTooltipCoroutine = null;
        }

        _currentHoveredElement = null;
        _currentTooltipText = null;

        if (_tooltipLabel != null) {
            _tooltipLabel.style.display = DisplayStyle.None;
        }
    }
}