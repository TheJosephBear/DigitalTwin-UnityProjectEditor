using System;
using System.Collections.Generic;
using SurveySystem;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyViewingUI : MonoBehaviour, ISurveyUIHandler {
    public List<QuestionTypeMapping> QuestionTypeMapping = new List<QuestionTypeMapping>();


    private SurveyBuildingManager _surveyBuildingManager;
    private SurveyBuilder _surveyBuilder;
    private VisualElement _root;
    private VisualElement _scrollViewContent;

    private List<SurveyQuestionUI> _addedQuestions = new List<SurveyQuestionUI>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _scrollViewContent = _root.Q<ScrollView>("survey-scroll-view").contentContainer;

        // Save button
        var saveButton = _root.Q<Button>("save-btn");
        saveButton.clicked += HandleSavePressed;
        // Exit button
        var exitButton = _root.Q<Button>("exit-btn");
        exitButton.clicked += HandleExitPressed;
    }

    public void Initialize(SurveyBuilder surveyBuilder, SurveyBuildingManager manager) {
        _surveyBuilder = surveyBuilder;
        _surveyBuildingManager = manager;
        // This will also add all of the UI components according to the survey structure
    }

    #region Input handling

    public void HandleQuestionAdded(string questionType, int insertAtIndex = -1) {
        var mapping = QuestionTypeMapping.Find(a => a.StringValue == questionType);
        VisualTreeAsset questionTemplate = mapping?.Template;
        TemplateContainer questionInstance = null;

        if (questionTemplate != null) {
            questionInstance = questionTemplate.Instantiate();
        } else {
            questionInstance = new TemplateContainer();
            questionInstance.Add(new Label($"Question template '{questionType}' is missing"));
        }

        QuestionType questionTypeEnum = mapping.EnumValue;
        QuestionBase addedQuestion = _surveyBuilder.AddNewQuestion(questionTypeEnum);

        var questionUI = new SurveyQuestionUI(
            questionInstance,
            addedQuestion.Id,
            this,
            questionTypeEnum,
            FindAnyObjectByType<ViewManager>().GetSerializedViewPointsList() // Do budoucna hledat líp (reference v mainManager)
        );

        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }
    }

    /// <summary>Returns the current index of the given question in the list, or -1 if not found.</summary>
    public int GetQuestionIndex(SurveyQuestionUI questionUI) {
        return _addedQuestions.IndexOf(questionUI);
    }

    public void HandleQuestionDeleted(int questionIndex) {
        // Intentionally empty: Voters cannot delete questions
    }

    public void HandleQuestionMoved(int questionIndex, int direction) {
        // Intentionally empty: Voters cannot move questions
    }

    public void HandleQuestionTitleChanged(int questionId, string newText) {
        // Intentionally empty: Voters cannot edit question titles
    }

    public void HandleQuestionDescriptionChanged(int questionId, string newText) {
        // Intentionally empty: Voters cannot edit question descriptions
    }

    public void HandleQuestionViewPointSelected(int questionID, string viewPointID) {
        _surveyBuilder.SetQuestionViewPoint(questionID, viewPointID);
        // In a viewer context, this might snap the camera to a view, but we don't save it to a builder
    }

    public void HandleAnswerAdded(int questionId) {
        // Intentionally empty: Voters cannot arbitrarily add new default answers
    }

    public void HandleAnswerOtherAdded(int questionId) {
        // Intentionally empty
    }

    public void HandleAnswerTextChanged(AnswerBase answer, string newText) {
        // If "Other" answer, you might want to save the user's custom string locally
    }

    public void HandleAnswerTextChanged(int questionId, int answerId, string newText) {
        // If "Other" answer, you might want to save the user's custom string locally
    }

    public void HandleAnswerRemoved(AnswerBase answer) {
        // Intentionally empty: Voters cannot remove answers
    }

    public void HandleSavePressed() {
        _surveyBuildingManager.SaveSurvey();
        // Normally HandleSubmitPressed for a viewer
    }

    public void HandleExitPressed() {
        if (_surveyBuildingManager != null) {
            _surveyBuildingManager.ExitSurveyCreation();
        }
    }

    #endregion
}
