using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [Header("Hover Events")]
    [Tooltip("Triggered when the mouse pointer enters the UI element.")]
    public UnityEvent OnHoverEnter;

    [Tooltip("Triggered when the mouse pointer exits the UI element.")]
    public UnityEvent OnHoverExit;

    public void OnPointerEnter(PointerEventData eventData) {
        OnHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnHoverExit?.Invoke();
    }
}
