using Cinemachine;
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

    private Dictionary<int, SurveyQuestionUIBase> _questionUICache = new();
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
        SurveyManager.Instance.ExitSurvey();
    }

    void HandleNextPressed() {
        ChangeQuestion(next: true);
        SurveyManager.Instance.SaveAnswers();
    }

    void HandlePreviousPressed() {
        ChangeQuestion(next: false);
        SurveyManager.Instance.SaveAnswers();
    }

    public void HandleAnswerSelected(int questionId, int answerId, bool isSelected) {
        print("ANSWER SELECTED");
        _responseManager.RegisterAnswer(questionId, answerId, isSelected);
    }

    public void HandleAnswerTextFilled(int questionId, int answerId, string newText) {
        print("TEXT FILLED");
        _responseManager.RegisterAnswer(questionId, answerId, true, newText);
    }

    #endregion

    void ChangeQuestion(bool next) {
        int indexToAdd = next ? 1 : -1;
        if (indexToAdd + _shownQuestionIndex >= _questions.Count || indexToAdd + _shownQuestionIndex < 0) return;
        _shownQuestionIndex += indexToAdd;

        _root.Q<Label>("page-count-label").text = (_shownQuestionIndex + 1).ToString() + "/" + _questions.Count;
        ClearQuestionFromUI();
        SurveyQuestionUIBase addedQuestionUI = AddQuestionToUI(_questions[_shownQuestionIndex]);

        addedQuestionUI.SetImageRender();

        print("trying to show " + _questions[_shownQuestionIndex].ViewPointId);
        if (MainManagerBase.Instance == null) return;
        if (_questions[_shownQuestionIndex].ViewPointId == "") {
        //    addedQuestionUI.SetImageRender();
            return;
        } 

        ViewManager viewManager = MainManagerBase.Instance.ViewManager;
        print(_questions[_shownQuestionIndex].ViewPointId);
        print(viewManager.GetViewPointByID(_questions[_shownQuestionIndex].ViewPointId).ID);
        viewManager.DeactivateViewPoint();
        viewManager.SetActiveViewPoint(
            viewManager.GetViewPointByID(_questions[_shownQuestionIndex].ViewPointId)
         );
        viewManager.ActivateViewPoint();
    }

    SurveyQuestionUIBase AddQuestionToUI(QuestionBase questionBase) {
        // Check if we already created this UI before
        if (_questionUICache.TryGetValue(questionBase.Id, out SurveyQuestionUIBase existingUI)) {
            // Show the existing one
            existingUI.QuestionElement.style.display = DisplayStyle.Flex;
            return existingUI;
        }

        // If not in cache, create it for the first time
        SurveyQuestionUIBase questionUI = _surveyUIBuilder.AddQuestionViewer(questionBase);
        questionUI.SetTitle(questionBase.Title);
        questionUI.SetDescription(questionBase.Description);
        questionUI.ImageID = questionBase.ImageID;

        if (questionBase is QuestionGridBase gridQuestion && questionUI is SurveyQuestionUIViewerGrid gridUI) {
            for (int i = 0; i < gridQuestion.GetColumnCount(); i++) {
                gridUI.AddColumn(gridQuestion.GetColumn(i));
            }

            for (int i = 0; i < gridQuestion.GetRowCount(); i++) {
                gridUI.AddRow(gridQuestion.GetRow(i));
            }

            gridUI.OnGridAnswerSelected += (qId, row, col, val) => {
                if (val) _responseManager.RegisterGridAnswer(qId, row, col);
            };
        } else if (questionUI is SurveyQuestionUIViewerString stringUI) {
            stringUI.OnAnswerSelected += HandleAnswerSelected;
            stringUI.OnAnswerTextFilled += HandleAnswerTextFilled;
            foreach (AnswerBase answer in questionBase.Answers) {
                questionUI.AddAnswer(answer.Text, answer.IsOther);
            }
        }

        // Add to cache
        _questionUICache.Add(questionBase.Id, questionUI);
        return questionUI;
    }

    void ClearQuestionFromUI() {
    //    _surveyUIBuilder.ClearScrollviewContent();

        // Instead of destroying, we just hide all cached questions
        foreach (var ui in _questionUICache.Values) {
            ui.QuestionElement.style.display = DisplayStyle.None;
        }
    }
}
