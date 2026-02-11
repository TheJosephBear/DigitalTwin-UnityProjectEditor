using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;
using System.Collections.Generic;

public class SurveyQuestionUI
{
    public int _questionID;
    private SurveyBuildingUI _surveyBuildingUIReff;
    private VisualElement _root;
    private QuestionType _questionType;
    
    // Track added answers - now using SurveyAnswerUI instead of VisualElements
    private List<SurveyAnswerUI> _addedAnswers = new List<SurveyAnswerUI>();
    private VisualElement _optionsList;
    
    // Reference to the answer template
    private VisualTreeAsset _answerTemplate;

    public SurveyQuestionUI(VisualElement root, int questionId, SurveyBuildingUI surveyBuildingUI, QuestionType questionType) {
        _root = root;
        _questionID = questionId;
        _surveyBuildingUIReff = surveyBuildingUI;
        _questionType = questionType;
        
        // Get the answer template from QuestionUIMapping
        QuestionUIMapping mapping = Object.FindFirstObjectByType<QuestionUIMapping>();
        if (mapping != null) {
            _answerTemplate = mapping.GetAnswerUITemplate(_questionType);
            if (_answerTemplate == null) {
                Debug.LogWarning($"No answer template found for question type: {_questionType}");
            }
        } else {
            Debug.LogError("QuestionUIMapping not found in scene!");
        }
        
        // Get the options list container
        _optionsList = _root.Q<RadioButtonGroup>("options-list");
        if (_optionsList == null) {
            // Try alternate container names if RadioButtonGroup isn't found
            _optionsList = _root.Q<VisualElement>("options-list");
        }
        
        RegisterInputs();
    }

    private void RegisterInputs() {
        var questionTitleField = _root.Q<TextField>("question-title-field");
        var questionDescriptionField = _root.Q<TextField>("question-description");
        var addOptionButton = _root.Q<Button>("add-option-button");
        var addOptionOtherButton = _root.Q<Button>("add-option-other-button");
        //var deleteQuestionButton = _root.Q<Button>("delete-question-button");
        //var requiredToggle = _root.Q<Toggle>("required-toggle");
        //var editQuestionButton = _root.Q<Button>("edit-question-button");

        questionTitleField.RegisterValueChangedCallback(evt => {
            _surveyBuildingUIReff.HandleQuestionTitleChanged(_questionID, evt.newValue);
        });

        questionDescriptionField.RegisterValueChangedCallback(evt => {
            _surveyBuildingUIReff.HandleQuestionDescriptionChanged(_questionID, evt.newValue);
        });

        addOptionButton.clicked += () => {
            _surveyBuildingUIReff.HandleAnswerAdded(_questionID);
            // After adding answer to data model, add UI element
            AddAnswerUI();
        };
        
        if (addOptionOtherButton != null) {
            addOptionOtherButton.clicked += () => {
                _surveyBuildingUIReff.HandleAnswerAdded(_questionID);
                // After adding "other" answer to data model, add UI element
                AddAnswerUI();
            };
        }
    }
    
    private void AddAnswerUI() {
        if (_optionsList == null) {
            Debug.LogWarning("Options list container not found!");
            return;
        }
        
        if (_answerTemplate == null) {
            Debug.LogWarning("Answer template not set!");
            return;
        }

        // Instantiate the answer template
        TemplateContainer answerElement = _answerTemplate.Instantiate();
        
        // Add the answer element to the list
        _optionsList.Add(answerElement);
        
        int answerIndex = _addedAnswers.Count;
        
        // Create SurveyAnswerUI instance to manage this answer
        SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, _surveyBuildingUIReff, this);
        _addedAnswers.Add(answerUI);
    }
    
    public void CloseCurrentModal() {
        // Close modals in all answers
        foreach (var answer in _addedAnswers) {
            answer.HideCurrentModal();
        }
    }
    
    public void DeleteAnswer(int answerIndex) {
        if (answerIndex < 0 || answerIndex >= _addedAnswers.Count) return;
        
        var answerUI = _addedAnswers[answerIndex];
        
        // Create answer object for removal
        var answer = new AnswerBase { Idx = answerIndex };
        _surveyBuildingUIReff.HandleAnswerRemoved(answer);
        
        // Remove from UI
        _optionsList.Remove(answerUI.AnswerElement);
        _addedAnswers.RemoveAt(answerIndex);
        
        // Update indices for all remaining answers
        for (int i = answerIndex; i < _addedAnswers.Count; i++) {
            _addedAnswers[i].UpdateIndex(i);
        }
    }
    
    public void MoveAnswerUp(int answerIndex) {
        if (answerIndex <= 0 || answerIndex >= _addedAnswers.Count) return;
        
        SwapAnswers(answerIndex, answerIndex - 1);
    }
    
    public void MoveAnswerDown(int answerIndex) {
        if (answerIndex < 0 || answerIndex >= _addedAnswers.Count - 1) return;
        
        SwapAnswers(answerIndex, answerIndex + 1);
    }
    
    private void SwapAnswers(int index1, int index2) {
        // Swap in the list
        var temp = _addedAnswers[index1];
        _addedAnswers[index1] = _addedAnswers[index2];
        _addedAnswers[index2] = temp;
        
        // Reorder in the visual container
        _optionsList.Clear();
        foreach (var answerUI in _addedAnswers) {
            _optionsList.Add(answerUI.AnswerElement);
        }
        
        // Update indices for all answers
        for (int i = 0; i < _addedAnswers.Count; i++) {
            _addedAnswers[i].UpdateIndex(i);
        }
        
        // Notify the survey builder about the reordering
        // You may need to add a method in SurveyBuildingUI to handle answer reordering
    }
}
