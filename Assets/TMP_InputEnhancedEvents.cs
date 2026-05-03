using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class TMP_InputEnhancedEvents : MonoBehaviour, IPointerClickHandler {
    [Header("Custom Events")]
    [SerializeField]
    public UnityEvent OnClickedOn;
    public UnityEvent OnClickedOutside;

    private TMP_InputField _inputField;
    private bool _isFocused;

    void Awake() {
        _inputField = GetComponent<TMP_InputField>();
    }

    // Detects when the Input Field itself is clicked
    public void OnPointerClick(PointerEventData eventData) {
        OnClickedOn?.Invoke();
        _isFocused = true;
    }

    void Update() {
        // Detects mouse click anywhere
        bool mouseClicked = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);

        if (mouseClicked && _isFocused) {
            // Check if the click was NOT on this GameObject
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(),
                Input.mousePosition,
                null)) {
                OnClickedOutside?.Invoke();
                _isFocused = false;
            }
        }
    }
}