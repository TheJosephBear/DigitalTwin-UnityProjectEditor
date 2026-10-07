using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class ContinuousGeoZoomSlider : MonoBehaviour, IEndDragHandler {
    private Slider _slider;

    // Set this to whatever default position you prefer (e.g. 0.5f for center-based sliders)
    [SerializeField] private float defaultSliderValue = 0.5f;

    private void Awake() {
        _slider = GetComponent<Slider>();

        // Listen to continuous dragging
        _slider.onValueChanged.AddListener(OnSliderValueChanged);

        // Set initial position
        _slider.SetValueWithoutNotify(defaultSliderValue);
    }

    private void OnDestroy() {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    // Called every frame while dragging
    private void OnSliderValueChanged(float value) {
        GeoMapLocalizationManager.Instance.ZoomMap(value);
    }

    // Called automatically by Unity EventSystem the moment you let go of the handle
    public void OnEndDrag(PointerEventData eventData) {
        // 1. Reset the slider visually WITHOUT firing OnSliderValueChanged
        _slider.SetValueWithoutNotify(defaultSliderValue);

        // 2. Reset the manager's baseline so the next drag starts fresh
        GeoMapManager.Instance.ResetZoomBaseline(defaultSliderValue);
    }
}