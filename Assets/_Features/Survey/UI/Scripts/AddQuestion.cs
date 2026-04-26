using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles question adding process. Showing the needed modal for question type selection and sending
/// the information to <see cref="SurveyUIControllerEditor"/>.
/// </summary>
public class AddQuestion : Singleton<AddQuestion> {

    private VisualElement _root;
    private SurveyUIControllerEditor _surveyBuildingUI;

    [SerializeField]
    private VisualTreeAsset _questionTypeSelectionTemplate;
    private TemplateContainer _questionTypeSelectionInstance;

    // Tracks which index to insert at when modal was opened from a specific bar (-1 = append)
    private int _pendingInsertIndex = -1;
    private SurveyUIControllerEditor _pendingBuildingUI;
    private VisualElement _pendingBar;

    public bool IsOpen { get => _questionTypeSelectionInstance != null && _questionTypeSelectionInstance.style.display == DisplayStyle.Flex; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _surveyBuildingUI = GetComponent<SurveyUIControllerEditor>();
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        _questionTypeSelectionInstance = _questionTypeSelectionTemplate.Instantiate();
        _questionTypeSelectionInstance.style.display = DisplayStyle.None;
    }

    private void OnRootPointerDown(PointerDownEvent evt) {
        // Only proceed if QuestionSelection is visible
        if (IsOpen) {
            // Check if the click target is outside the QuestionSelection container
            if (!_questionTypeSelectionInstance.ContainsPoint(_questionTypeSelectionInstance.WorldToLocal(evt.position))) {
                HideModal();
            }
        }
    }

    /// <summary>
    /// Opens the question-type selection modal. When a type is chosen, the question will be
    /// inserted at <paramref name="insertIndex"/> in <paramref name="buildingUI"/>.
    /// The modal is reparented into <paramref name="bar"/> while open.
    /// </summary>
    public void OpenModalAtIndex(int insertIndex, VisualElement bar) {
        _pendingInsertIndex = insertIndex;
        _pendingBuildingUI = GetComponent<SurveyUIControllerEditor>();
        _pendingBar = bar;
        ShowModal();
    }

    private void ShowModal() {
        // Move the modal into the triggering bar so it appears anchored to it
        _questionTypeSelectionInstance.RemoveFromHierarchy();
        _root.Add(_questionTypeSelectionInstance);

        _questionTypeSelectionInstance.style.display = DisplayStyle.Flex;
        _questionTypeSelectionInstance.style.position = Position.Absolute;
        Vector2 pendingBarButtonPos = _pendingBar.Q<Button>().worldBound.center;
        Vector2 buttonCenter = new Vector2(pendingBarButtonPos.x, pendingBarButtonPos.y);
        _questionTypeSelectionInstance.style.left = Mathf.Ceil(buttonCenter.x);
        _questionTypeSelectionInstance.style.top = Mathf.Ceil(buttonCenter.y);
        _questionTypeSelectionInstance.BringToFront();

        List<Button> buttons = _questionTypeSelectionInstance.Query<Button>().ToList();
        buttons.ForEach(button => {
            button.clicked += () => {
                Debug.Log($"Add question of type: {button.name}");
                AddQuestionByName(button.name);
                HideModal();
            };
        });

        // Register callback on the global root to detect clicks anywhere in the document
        _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    private void AddQuestionByName(string name) {
        SurveyUIControllerEditor target = _pendingBuildingUI ?? _surveyBuildingUI;
        target.HandleQuestionAdded(name, _pendingInsertIndex);
    }

    private void HideModal() {
        _questionTypeSelectionInstance.style.display = DisplayStyle.None;

        List<Button> buttons = _questionTypeSelectionInstance.Query<Button>().ToList();
        buttons.ForEach(button => {
            button.clickable = null;
        });

        // Move the modal back to root so it is not destroyed when bars are rebuilt
        _questionTypeSelectionInstance.RemoveFromHierarchy();

        _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        _pendingInsertIndex = -1;
        _pendingBuildingUI = null;
        _pendingBar = null;
    }

    public void SetInsertIndex(int index) {
        _pendingInsertIndex = index;
    }

    public void IncrementInsertIndex(int amount = 1) {
        // Only increment if we aren't in "Append Mode" (-1)
        if (_pendingInsertIndex != -1) {
            _pendingInsertIndex += amount;
        }
    }

    private void OnEnable() {
        // Re-register the callback when the object is enabled
        if (_root != null && IsOpen) {
            // Register callback on the global root to detect clicks anywhere in the document
            _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }

    private void OnDisable() {
        // Clean up the callback when the object is disabled
        if (_root != null) {
            _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }

    private void OnDestroy() {
        // Clean up the callback when the object is destroyed
        if (_root != null) {
            _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }
}
