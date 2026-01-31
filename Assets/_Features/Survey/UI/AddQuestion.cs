using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AddQuestion : Singleton<AddQuestion>
{
    private VisualElement _root;
    private VisualElement _addQuestionBar;
    private TemplateContainer _questionSelection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _addQuestionBar = _root.Q<TemplateContainer>("AddQuestionBar");
        _questionSelection = _addQuestionBar.Q<TemplateContainer>("QuestionSelection");

        _addQuestionBar.Q<Button>("AddQuestionButton").clicked += OpenModal;
    }

    private void OnRootPointerDown(PointerDownEvent evt)
    {
        // Only proceed if QuestionSelection is visible
        if (_questionSelection.style.display == DisplayStyle.Flex)
        {
            // Check if the click target is outside the QuestionSelection container
            if (!_questionSelection.ContainsPoint(_questionSelection.WorldToLocal(evt.position)))
            {
                HideModal();
            }
        }
    }

    private void OpenModal()
    {
        _questionSelection.style.display = DisplayStyle.Flex;
        List<Button> buttons = _questionSelection.Query<Button>().ToList();
        buttons.ForEach(button =>
        {
            button.clicked += () =>
            {
                Debug.Log($"Add question of type: {button.name}");
                AddQuestionByName(button.name);
                HideModal();
            };
        });
        // Register callback on the global root to detect clicks anywhere in the document
        _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    private void AddQuestionByName(string name)
    {
        SurveyController.Instance.AddQuestion(name);
        // Placeholder for adding question logic
        Debug.Log("Question added.");
    }

    private void HideModal()
    {
       _questionSelection.style.display = DisplayStyle.None;
        List<Button> buttons = _questionSelection.Query<Button>().ToList();
        buttons.ForEach(button =>
        {
            button.clickable = null;
        });
        _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    private void OnEnable()
    {
        // Re-register the callback when the object is enabled
        if (_root != null)
        {
            // Register callback on the global root to detect clicks anywhere in the document
            _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }

    private void OnDisable()
    {
        // Clean up the callback when the object is disabled
        if (_root != null)
        {
            _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }

    private void OnDestroy()
    {
        // Clean up the callback when the object is destroyed
        if (_root != null)
        {
            _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }
}
