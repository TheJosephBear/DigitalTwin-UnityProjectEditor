using UnityEngine;
using UnityEngine.UI;

public class NoteMarkerUI : MonoBehaviour {
    public Button Button;
    public RectTransform RectTransform;

    private ClickNote _note;
    private NoteAddingManager _manager;

    public void Initialize(ClickNote note, NoteAddingManager manager) {
        _note = note;
        _manager = manager;

        Button.onClick.AddListener(OnClicked);
    }

    private void OnClicked() {
        _manager.OpenEditPanel(_note);
    }
}