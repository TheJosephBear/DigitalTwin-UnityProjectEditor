using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PinHUDButton : MonoBehaviour {

    public PinUI UIreff;
    public PinObject PinRefference;
    public Image ButtonImageRef;
    public TextMeshProUGUI NameTextRef;

    [SerializeField] float _inactiveHeight = 100f;
    [SerializeField] float _activeHeight = 200f;
    [SerializeField] GameObject ArrowsContainer;
    [SerializeField] GameObject ArrowUpButton;
    [SerializeField] GameObject ArrowDownButton;
    bool _isActivePin;


    public void Initialize(PinUI ui, PinObject pin) {
        UIreff = ui;
        PinRefference = pin;
        if (NameTextRef != null) NameTextRef.text = pin.Name;
        _isActivePin = PinManager.Instance.GetActivePin() == PinRefference;
        ToggleVisual(_isActivePin);
        ToggleMovableVisual(_isActivePin);
    }

    public void OnClick() {
        UIreff.OnHUDButtonClick(PinRefference);
    }

    public void ToggleVisual(bool toggleOn) {
        string color = "#FFFFFF";
        if (toggleOn) {
            color = "#FF92FE";
        }

        if (ColorUtility.TryParseHtmlString(color, out Color newColor)) {
            ButtonImageRef.color = newColor;
        }
    }

    public void OnArrowUp() {
        UIreff.OnMoveButton(PinManager.Instance.GetPins().IndexOf(PinRefference), moveUp: true);
    }

    public void OnArrowDown() {
        UIreff.OnMoveButton(PinManager.Instance.GetPins().IndexOf(PinRefference), moveUp: false);
    }

    public void ToggleMovableVisual(bool toggleOn) {
        if (MainManagerBase.Instance is ViewingManager) {
            toggleOn = false;
        }

        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 size = rectTransform.sizeDelta;
        size.y = toggleOn ? _activeHeight : _inactiveHeight;
        rectTransform.sizeDelta = size;

        if (ArrowsContainer != null) ArrowsContainer.SetActive(toggleOn);
    }
}
