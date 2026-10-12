using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [TextArea(2, 5)]
    [SerializeField] private string tooltipContent;

    public void OnPointerEnter(PointerEventData eventData) {
        TooltipManager.Instance.ShowTooltip(tooltipContent);
    }

    public void OnPointerExit(PointerEventData eventData) {
        TooltipManager.Instance.HideTooltip();
    }

    private void OnDisable() {
        // Safety check if the object gets hidden while hovered
        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip();
    }
}
