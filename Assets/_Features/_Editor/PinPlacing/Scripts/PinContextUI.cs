using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PinContextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 50f);

    private RectTransform rectTransform;
    private PinObject targetPin;
    private Camera mainCamera;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(PinObject pin) {
        targetPin = pin;
        mainCamera = Camera.main;

        if (titleText != null) titleText.text = pin.Name;
        if (descriptionText != null) descriptionText.text = pin.Description;
        if (iconImage != null) {
            iconImage.sprite = pin.Image;
            iconImage.gameObject.SetActive(pin.Image != null);
        }
    }
}
