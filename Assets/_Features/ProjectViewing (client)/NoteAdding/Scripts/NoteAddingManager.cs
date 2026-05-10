using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles:
/// - Clicking in world space to create notes
/// - Displaying note markers as UI circles
/// - Keeping markers attached to world positions
/// - Opening an edit panel when clicking a note
/// </summary>
public class NoteAddingManager : MonoBehaviour {
    [Header("General")]
    public Camera TargetCamera;
    public bool ShowingNotes = true;

    [Header("Raycast")]
    public LayerMask PlacementLayers = ~0;
    public float MaxRayDistance = 500f;

    [Header("UI")]
    public Canvas MainCanvas;

    [Tooltip("Small circle UI prefab")]
    public NoteMarkerUI MarkerPrefab;

    [Tooltip("Parent for spawned markers")]
    public RectTransform MarkerContainer;

    [Header("Edit UI")]
    public GameObject EditPanel;
    public TMP_InputField NameInput;
    public TMP_InputField DescriptionInput;
    public Button SaveButton;
    public Button DeleteButton;

    private readonly List<ClickNote> _noteList = new();
    private readonly Dictionary<ClickNote, NoteMarkerUI> _spawnedMarkers = new();

    private ClickNote _currentlyEditing;

    private void Awake() {
        if (TargetCamera == null)
            TargetCamera = Camera.main;

        EditPanel.SetActive(false);

        SaveButton.onClick.AddListener(SaveCurrentNote);
        DeleteButton.onClick.AddListener(DeleteCurrentNote);
    }

    private void Update() {
        HandleCreateNoteInput();
        UpdateMarkers();
    }

    // =========================================================
    // NOTE CREATION
    // =========================================================

    private void HandleCreateNoteInput() {
        if (!ShowingNotes)
            return;

        // Left mouse click
        if (Input.GetMouseButtonDown(0)) {
            // Ignore clicking through UI
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = TargetCamera.ScreenPointToRay(Input.mousePosition);

            print("Before creation");
            if (Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance, PlacementLayers)) {
                CreateNote(hit.point);
            }
        }
    }

    private void CreateNote(Vector3 worldPosition) {
        print("Created");
        ClickNote newNote = new ClickNote {
            ID = Guid.NewGuid().ToString(),
            Name = "New Note",
            Description = "",
            Position = worldPosition
        };

        _noteList.Add(newNote);

        SpawnMarker(newNote);

        OpenEditPanel(newNote);

        Debug.Log($"Created note at: {worldPosition}");
    }

    // =========================================================
    // MARKERS
    // =========================================================

    private void SpawnMarker(ClickNote note) {
        NoteMarkerUI marker = Instantiate(MarkerPrefab, MarkerContainer);

        marker.Initialize(note, this);

        _spawnedMarkers.Add(note, marker);
    }

    private void UpdateMarkers() {
        foreach (var pair in _spawnedMarkers) {
            ClickNote note = pair.Key;
            NoteMarkerUI marker = pair.Value;

            if (!ShowingNotes) {
                marker.gameObject.SetActive(false);
                continue;
            }

            marker.gameObject.SetActive(true);

            Vector3 screenPos = TargetCamera.WorldToScreenPoint(note.Position);

            // Hide if behind camera
            bool visible = screenPos.z > 0;

            marker.gameObject.SetActive(visible);

            if (!visible)
                continue;

            marker.RectTransform.position = screenPos;

            // Optional scaling based on distance
            float distance = Vector3.Distance(TargetCamera.transform.position, note.Position);

            float scale = Mathf.Clamp(10f / distance, 0.5f, 1.2f);

            marker.RectTransform.localScale = Vector3.one * scale;
        }
    }

    // =========================================================
    // EDITING
    // =========================================================

    public void OpenEditPanel(ClickNote note) {
        _currentlyEditing = note;

        EditPanel.SetActive(true);

        NameInput.text = note.Name;
        DescriptionInput.text = note.Description;
    }

    private void SaveCurrentNote() {
        if (_currentlyEditing == null)
            return;

        _currentlyEditing.Name = NameInput.text;
        _currentlyEditing.Description = DescriptionInput.text;

        Debug.Log($"Saved note: {_currentlyEditing.Name}");
    }

    private void DeleteCurrentNote() {
        if (_currentlyEditing == null)
            return;

        if (_spawnedMarkers.TryGetValue(_currentlyEditing, out NoteMarkerUI marker)) {
            Destroy(marker.gameObject);
            _spawnedMarkers.Remove(_currentlyEditing);
        }

        _noteList.Remove(_currentlyEditing);

        Debug.Log($"Deleted note: {_currentlyEditing.Name}");

        _currentlyEditing = null;

        EditPanel.SetActive(false);
    }
}

[Serializable]
public class ClickNote {
    public string ID;
    public string Name;
    public string Description;

    public Vector3 Position;
}