using System.Collections.Generic;
using UnityEngine;

public class PinObject: EditorObjectBase, IClickable {
    [Header("Pin Data")]
    public Sprite Image;

    [Header("Outline")]
    [SerializeField] private Material _outlineMaterial; // Added as an extra material while the pin is being edited

    [Header("Hover Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 originalScale;
    private bool _outlined = false;

    private void Awake() {
        originalScale = transform.localScale;
    }

    public void OnClickDown() {
        if (PinManager.Instance.IsPlacing) return;
        PinManager.Instance.SelectPin(this);
    }

    public void OnClick() { }
    public void OnClickUp() { }

    public void OnHover() {
        transform.localScale = Vector3.Scale(originalScale, hoverScale);
    }

    public void OnUnhover() {
        transform.localScale = originalScale;
    }

    public void SetOutline(bool toggleOn) {
        if (_outlineMaterial == null || _outlined == toggleOn) return;
        _outlined = toggleOn;

        foreach (Renderer rend in GetComponentsInChildren<Renderer>()) {
            List<Material> materials = new List<Material>(rend.sharedMaterials);
            if (toggleOn) materials.Add(_outlineMaterial);
            else materials.Remove(_outlineMaterial);
            rend.sharedMaterials = materials.ToArray();
        }
    }

    /// <summary>
    /// While placing, the pin follows the cursor, so its colliders must not block the placing raycast.
    /// </summary>
    public void SetPlacingMode(bool placing) {
        foreach (Collider col in GetComponentsInChildren<Collider>()) {
            col.enabled = !placing;
        }
        if (placing) OnUnhover();
    }

    public SerializablePin Serialize() {
        return new SerializablePin {
            ID = ID,
            Name = Name,
            Description = Description,
            position = transform.position
        };
    }

    public void Deserialize(SerializablePin data) {
        ID = data.ID;
        SetName(data.Name);
        SetDescription(data.Description);
        transform.position = data.position;
    }
}
