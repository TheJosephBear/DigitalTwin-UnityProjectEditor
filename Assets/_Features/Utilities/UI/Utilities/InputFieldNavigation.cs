using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tab / Shift+Tab moves from the text field being typed in to the next / previous one on the same canvas
/// (top to bottom, left to right). Focus() puts the caret in a field so the user can type right away.
/// Created automatically at startup, nothing needs to be placed in a scene.
/// </summary>
public class InputFieldNavigation : MonoBehaviour {

    const float RowTolerance = 10f; // Fields this close vertically (in pixels) count as one row

    static InputFieldNavigation _instance;

    TMP_InputField _pendingFocus;
    int _pendingFocusFrame;
    readonly List<TMP_InputField> _fields = new List<TMP_InputField>();
    readonly Vector3[] _corners = new Vector3[4];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create() {
        if (_instance != null) return;
        var go = new GameObject(nameof(InputFieldNavigation));
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<InputFieldNavigation>();
    }

    /// <summary>
    /// Focuses the field on the next frame, once the UI that shows it is set up and the click that opened it is handled.
    /// </summary>
    public static void Focus(TMP_InputField field) {
        if (field == null) return;
        if (_instance == null) Create();
        _instance._pendingFocus = field;
        _instance._pendingFocusFrame = Time.frameCount;
    }

    void Update() {
        if (_pendingFocus != null && Time.frameCount > _pendingFocusFrame) {
            TMP_InputField field = _pendingFocus;
            _pendingFocus = null;
            Activate(field);
        }
    }

    // LateUpdate: the EventSystem has already handled this frame's keys, so a multi-line field has taken its tab character
    void LateUpdate() {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
            Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null || eventSystem.currentSelectedGameObject == null) return;

        TMP_InputField current = eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>();
        if (current == null || !current.isFocused) return;

        bool backwards = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        TMP_InputField next = FindNeighbour(current, backwards);
        if (next == null) return;

        RemoveTypedTab(current);
        Activate(next);
    }

    TMP_InputField FindNeighbour(TMP_InputField current, bool backwards) {
        Canvas canvas = current.GetComponentInParent<Canvas>();
        Canvas root = canvas != null ? canvas.rootCanvas : null;
        Transform scope = root != null ? root.transform : current.transform.root;
        Camera cam = root != null && root.renderMode != RenderMode.ScreenSpaceOverlay ? root.worldCamera : null;

        scope.GetComponentsInChildren(false, _fields);
        _fields.RemoveAll(f => f != current && (f.readOnly || !f.IsInteractable() || !IsVisible(f)));
        if (_fields.Count < 2) return null;

        var keys = new Dictionary<TMP_InputField, Vector2>();
        foreach (TMP_InputField field in _fields) {
            ((RectTransform)field.transform).GetWorldCorners(_corners);
            Vector2 topLeft = RectTransformUtility.WorldToScreenPoint(cam, _corners[1]);
            keys[field] = new Vector2(Mathf.Round(-topLeft.y / RowTolerance), topLeft.x);
        }
        _fields.Sort((a, b) => {
            int byRow = keys[a].x.CompareTo(keys[b].x);
            return byRow != 0 ? byRow : keys[a].y.CompareTo(keys[b].y);
        });

        int index = _fields.IndexOf(current);
        int step = backwards ? -1 : 1;
        return _fields[(index + step + _fields.Count) % _fields.Count];
    }

    // A multi-line field types the tab as a character before we move away from it
    static void RemoveTypedTab(TMP_InputField field) {
        if (field.lineType == TMP_InputField.LineType.SingleLine) return;

        string text = field.text;
        int position = field.stringPosition;
        if (position > 0 && position <= text.Length && text[position - 1] == '\t') {
            field.text = text.Remove(position - 1, 1);
        }
    }

    static void Activate(TMP_InputField field) {
        if (field == null || !field.isActiveAndEnabled || !field.IsInteractable() || !IsVisible(field)) return;

        field.Select();
        field.ActivateInputField();
    }

    // UIBehaviour hides a UI by turning its canvas off, the fields stay active
    static bool IsVisible(TMP_InputField field) {
        foreach (Canvas canvas in field.GetComponentsInParent<Canvas>()) {
            if (!canvas.enabled) return false;
        }
        return true;
    }
}
