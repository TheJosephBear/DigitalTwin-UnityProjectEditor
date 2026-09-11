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
        // Move the modal into root so it appears anchored above everything
        _questionTypeSelectionInstance.RemoveFromHierarchy();
        _root.Add(_questionTypeSelectionInstance);

        _questionTypeSelectionInstance.style.display = DisplayStyle.Flex;
        _questionTypeSelectionInstance.style.position = Position.Absolute;
        
        Vector2 pendingBarButtonPos = _pendingBar != null && _pendingBar.Q<Button>() != null 
            ? _pendingBar.Q<Button>().worldBound.center 
            : new Vector2(_root.layout.width / 2f, _root.layout.height / 2f);

        float defaultWidth = 250f;
        float defaultHeight = 330f;
        float modalWidth = _questionTypeSelectionInstance.layout.width > 0 ? _questionTypeSelectionInstance.layout.width : defaultWidth;
        float modalHeight = _questionTypeSelectionInstance.layout.height > 0 ? _questionTypeSelectionInstance.layout.height : defaultHeight;

        // Position horizontally centered under the + button
        float targetX = pendingBarButtonPos.x - (modalWidth / 2f);
        float targetY = pendingBarButtonPos.y + 22f;

        if (_root.layout.height > 0 && targetY + modalHeight > _root.layout.height - 20f) {
            float aboveY = pendingBarButtonPos.y - modalHeight - 22f;
            if (aboveY >= 10f) {
                targetY = aboveY;
            }
        }

        float maxX = Mathf.Max(0, _root.layout.width - modalWidth);
        float maxY = Mathf.Max(0, _root.layout.height - modalHeight);

        _questionTypeSelectionInstance.style.left = Mathf.Clamp(targetX, 16f, Mathf.Max(16f, maxX - 16f));
        _questionTypeSelectionInstance.style.top = Mathf.Clamp(targetY, 16f, Mathf.Max(16f, maxY - 16f));
        _questionTypeSelectionInstance.BringToFront();

        // Ensure the modal stays precisely centered and within bounds after layout pass
        _questionTypeSelectionInstance.schedule.Execute(() => {
            if (_pendingBar == null) return;
            Button barButton = _pendingBar.Q<Button>();
            if (barButton == null) return;

            Vector2 barCenter = barButton.worldBound.center;
            float resolvedWidth = _questionTypeSelectionInstance.layout.width > 0 ? _questionTypeSelectionInstance.layout.width : defaultWidth;
            float resolvedHeight = _questionTypeSelectionInstance.layout.height > 0 ? _questionTypeSelectionInstance.layout.height : defaultHeight;

            float exactTargetX = barCenter.x - (resolvedWidth / 2f);
            float exactTargetY = barCenter.y + 22f;

            if (_root.layout.height > 0 && exactTargetY + resolvedHeight > _root.layout.height - 20f) {
                float aboveY = barCenter.y - resolvedHeight - 22f;
                if (aboveY >= 10f) {
                    exactTargetY = aboveY;
                }
            }

            float resolvedMaxX = Mathf.Max(0, _root.layout.width - resolvedWidth);
            float resolvedMaxY = Mathf.Max(0, _root.layout.height - resolvedHeight);

            _questionTypeSelectionInstance.style.left = Mathf.Clamp(exactTargetX, 16f, Mathf.Max(16f, resolvedMaxX - 16f));
            _questionTypeSelectionInstance.style.top = Mathf.Clamp(exactTargetY, 16f, Mathf.Max(16f, resolvedMaxY - 16f));
        });

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
