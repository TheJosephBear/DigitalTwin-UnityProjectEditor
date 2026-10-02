using System;
using UnityEngine;

public class PinObject: EditorObjectBase, IClickable {
    [Header("Pin Data")]
    public string Name;
    public string Description;
    public Sprite Image;
    public Vector3 Position;

    [Header("Hover Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 originalScale;

    private void Awake() {
        originalScale = transform.localScale;
    }

    public void OnClick() {
        PinManager.Instance.SelectPin(this);
    }

    public void OnClickDown() { }
    public void OnClickUp() { }

    public void OnHover() {
        transform.localScale = Vector3.Scale(originalScale, hoverScale);
    }

    public void OnUnhover() {
        transform.localScale = originalScale;
    }

    public SerializablePin Serialize() {
        return new SerializablePin {
            ID = ID,
            Name = Name,
            Description = Description,
            Position = transform.position
        };
    }

    public void Deserialize(SerializablePin data) {
        ID = data.ID;
        Name = data.Name;
        Description = data.Description;
        transform.position = data.Position;
    }
}
