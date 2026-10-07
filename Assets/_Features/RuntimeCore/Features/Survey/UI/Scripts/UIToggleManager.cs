using UnityEngine;
using UnityEngine.UIElements;

public class UIToggleManager : MonoBehaviour {
    public UIDocument UIDocument;

    private bool _isVisible = true;

    private void Awake() {
        if (UIDocument == null) UIDocument = GetComponent<UIDocument>();
    }

    /// <summary>
    /// Toggles the UI visibility by changing the root element's display style.
    /// </summary>
    /// <param name="show">True to render (Flex), false to hide (None).</param>
    public void ToggleUIVisibility(bool show) {
        if (UIDocument == null || UIDocument.rootVisualElement == null) return;

        _isVisible = show;

        // DisplayStyle.Flex makes it visible and part of the layout
        // DisplayStyle.None hides it completely and removes it from layout calculations
        UIDocument.rootVisualElement.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SwitchView() {
        ToggleUIVisibility(!_isVisible);
    }
}