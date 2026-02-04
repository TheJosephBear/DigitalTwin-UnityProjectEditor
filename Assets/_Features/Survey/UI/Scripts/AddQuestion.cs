using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Handles question adding process. Showing the needed modal for question type selection and sending 
/// the information to <see cref="SurveyBuildingUI"/>.
/// </summary>
public class AddQuestion : Singleton<AddQuestion> {

    private VisualElement _root;
    private VisualElement _addQuestionBar;
    private TemplateContainer _questionSelection;
    private SurveyBuildingUI _surveyBuildingUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _surveyBuildingUI = GetComponent<SurveyBuildingUI>();
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _addQuestionBar = _root.Q<TemplateContainer>("add-question-bar");
        _questionSelection = _addQuestionBar.Q<TemplateContainer>("question-selection");

        _addQuestionBar.Q<Button>("add-question-button").clicked += OpenModal;
    }

    private void OnRootPointerDown(PointerDownEvent evt) {
        // Only proceed if QuestionSelection is visible
        if (_questionSelection.style.display == DisplayStyle.Flex) {
            // Check if the click target is outside the QuestionSelection container
            if (!_questionSelection.ContainsPoint(_questionSelection.WorldToLocal(evt.position))) {
                HideModal();
            }
        }
    }

    private void OpenModal() {
        _questionSelection.style.display = DisplayStyle.Flex;
        List<Button> buttons = _questionSelection.Query<Button>().ToList();
        buttons.ForEach(button => {
            button.clicked += () => {
                Debug.Log($"Add question of type: {button.name}");
                AddQuestionByName(button.name);
                HideModal();
            };
        });
        _questionSelection.BringToFront();
        // Register callback on the global root to detect clicks anywhere in the document
        _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    private void AddQuestionByName(string name) {
        _surveyBuildingUI.HandleQuestionAdded(name);
        // Placeholder for adding question logic
        Debug.Log("Question added.");
    }

    private void HideModal() {
        _questionSelection.style.display = DisplayStyle.None;
        List<Button> buttons = _questionSelection.Query<Button>().ToList();
        buttons.ForEach(button => {
            button.clickable = null;
        });
        _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    private void OnEnable() {
        // Re-register the callback when the object is enabled
        if (_root != null) {
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
