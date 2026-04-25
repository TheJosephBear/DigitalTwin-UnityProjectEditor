using SurveySystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyUIControllerViewer : MonoBehaviour {
    private VisualElement _root;
    private SurveyManager _surveyManager;
    private SurveyBuilder _surveyBuilder; // Interface for data model
    private SurveyResponseManager _responseManager; // Handles response data model
    private SurveyUIBuilder _surveyUIBuilder; // Script adding template instances to UI


    private List<QuestionBase> _questions;
    private int _shownQuestionIndex = -1;

    public void Initialize(SurveyBuilder surveyBuilder, SurveyResponseManager responseManager, SurveyManager manager) {
        _surveyBuilder = surveyBuilder;
        _responseManager = responseManager;
        _surveyManager = manager;

        Survey survey = _surveyBuilder.GetActiveSurvey();
        _questions = survey.GetAllQuestions();


        _surveyUIBuilder = GetComponent<SurveyUIBuilder>();
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        #region Button setup

        var toggleButton = _root.Q<Button>("toggle-btn");
        toggleButton.clicked += HandleTogglePressed;

        var prevButton = _root.Q<Button>("previous-btn");
        prevButton.clicked += HandlePreviousPressed;

        var nextButton = _root.Q<Button>("next-btn");
        nextButton.clicked += HandleNextPressed;

        #endregion

        ChangeQuestion(true);
    }

    #region Input handling

    void HandleTogglePressed() {
        // Hide/Show Survey UI
    }

    void HandleNextPressed() {
        ChangeQuestion(next: true);
    }

    void HandlePreviousPressed() {
        ChangeQuestion(next: false);
    }

    public void HandleAnswerSelected(int questionId, int answerId) {
        _responseManager.RegisterAnswer(questionId, answerId);
    }

    public void HandleAnswerTextFilled(int questionId, int answerId, string newText) {

    }

    #endregion

    void ChangeQuestion(bool next) {
        int indexToAdd = next ? 1 : -1;
        if (indexToAdd + _shownQuestionIndex >= _questions.Count || indexToAdd + _shownQuestionIndex < 0) return;
        _shownQuestionIndex += indexToAdd;

        _root.Q<Label>("page-count-label").text = (_shownQuestionIndex+1).ToString() + "/" + _questions.Count;
        ClearQuestionFromUI();
        AddQuestionToUI(_questions[_shownQuestionIndex]);

    //    print(_responseManager.ExportResponseJson());
    }

    void AddQuestionToUI(QuestionBase questionBase) {
        /*
        ISurveyQuestionUI questionUI = _surveyUIBuilder.AddQuestionViewer(questionBase);
        questionUI.SetTitle(questionBase.Title);
        questionUI.SetDescription(questionBase.Description);
        foreach (AnswerBase answer in questionBase.Answers) {
            questionUI.AddAnswer(answer.Text, answer.IsOther);
        }

        if (questionUI is ISurveyQuestionViewerUI viewerUI) {
            viewerUI.OnAnswerSelected += HandleAnswerSelected;
            viewerUI.OnAnswerTextFilled += HandleAnswerTextFilled;
        }
        */
    }

    void ClearQuestionFromUI() {
        _surveyUIBuilder.ClearScrollviewContent();
    }
}
