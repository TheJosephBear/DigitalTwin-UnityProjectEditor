using System;
using System.Collections.Generic;
using System.Linq;
using SurveySystem;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Handles the Survey Builder UI logic. 
/// Receives input events from addedQuestion and answer fields, as well as buttons, 
/// and relays the changes to the <see cref="SurveyBuilder"/> for updating the survey data model.
/// (Tohle by mo�n� mohl d�lat sv�j vlastn� script) -> Manages the instantiation of addedQuestion UI elements based on templates and keeps track of added questions.
/// </summary>
public class SurveyUIControllerEditor : MonoBehaviour {

    private VisualElement _root;
    private SurveyManager _surveyManager;
    private SurveyBuilder _surveyBuilder; // Interface for data model
    private SurveyUIBuilder _surveyUIBuilder; // Script adding template instances to UI

    void Awake() {
        _surveyUIBuilder = GetComponent<SurveyUIBuilder>();
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        // Save button
        var saveButton = _root.Q<Button>("save-btn");
        saveButton.clicked += HandleSavePressed;
        // Exit button
        var exitButton = _root.Q<Button>("exit-btn");
        exitButton.clicked += HandleExitPressed;
    }

    public void Initialize(SurveyBuilder surveyBuilder, SurveyManager manager) {
        _surveyBuilder = surveyBuilder;
        _surveyManager = manager;
    }

    #region Input handling

    #region Survey Building

    public void HandleQuestionAdded(string questionTypeString, int insertAtIndex = -1) {
        QuestionTypeMapping mapping = _surveyUIBuilder.questionUIMapping.GetMappingByQuestionType(questionTypeString);
        if (mapping == null) {
            Debug.LogError($"No mapping found for string: {questionTypeString}");
            return;
        }

        QuestionType questionTypeEnum = mapping.QuestionType;
        HandleQuestionAdded(questionTypeEnum, insertAtIndex);
    }

    public SurveyQuestionUIBase HandleQuestionAdded(QuestionType questionType, int insertAtIndex = -1) {
        QuestionBase newQuestion = _surveyBuilder.AddNewQuestion(questionType);
        return HandleExistingQuestionAdded(newQuestion, insertAtIndex);
    }

    public SurveyQuestionUIBase HandleExistingQuestionAdded(QuestionBase question, int insertAtIndex = -1, bool isDeserialized = false) {
        SurveyQuestionUIBase addedQuestionUI = _surveyUIBuilder.AddQuestionEditor(question, insertAtIndex: insertAtIndex, isDeserialized: isDeserialized);

        if (addedQuestionUI is SurveyQuestionUIEditor editorUI) {
            editorUI.OnTitleChanged += HandleQuestionTitleChanged;
            editorUI.OnDescriptionChanged += HandleQuestionDescriptionChanged;
            editorUI.OnQuestionDeleted += HandleQuestionDeleted;
            editorUI.OnQuestionMoved += HandleQuestionMoved;
            editorUI.OnViewpointSelected += HandleQuestionViewPointSelected;
        }

        if (addedQuestionUI is SurveyQuestionUIEditorGrid gridUI) {
            gridUI.OnAddRow += AddRow;
            gridUI.OnAddColumn += AddColumn;
        } else if (addedQuestionUI is SurveyQuestionUIEditorString builderUI) {
            builderUI.OnAnswerAdded += HandleAnswerAdded;
            builderUI.OnAnswerOtherAdded += HandleAnswerOtherAdded;
            builderUI.OnAnswerRemoved += HandleAnswerRemoved;

            if (!isDeserialized) {
                builderUI.AddInitialAnswer();
            }
        }

        return addedQuestionUI;
    }

    void AddRow(int questionID, SurveyAnswerUIEditorGrid answerUI) {
        _surveyBuilder.AddRow(questionID);
        answerUI.OnTextChanged += OnRowTextChanged;
    }

    void AddColumn(int questionID, SurveyAnswerUIEditorGrid answerUI) {
        _surveyBuilder.AddColumn(questionID);
        answerUI.OnTextChanged += OnColumnTextChanged;
    }

    void OnRowTextChanged(int questionID, int rowIdx, string text) {
        _surveyBuilder.SetRowText(questionID, rowIdx, text);
    }

    void OnColumnTextChanged(int questionID, int columnIdx, string text) {
        _surveyBuilder.SetColumnText(questionID, columnIdx, text);
    }

    public void HandleQuestionDeleted(int questionIndex) {
        if (!_surveyUIBuilder.DeleteQuestion(questionIndex)) return;
        _surveyBuilder.RemoveQuestion(questionIndex);
    }

    public void HandleQuestionMoved(int questionIndex, int direction) {
        _surveyUIBuilder.MoveQuestion(questionIndex, direction);
    }

    public void HandleQuestionTitleChanged(int questionId, string newText) {
        _surveyBuilder.SetQuestionTitle(questionId, newText);
    }

    public void HandleQuestionDescriptionChanged(int questionId, string newText) {
        _surveyBuilder.SetQuestionDescription(questionId, newText);
    }

    public void HandleQuestionViewPointSelected(int questionID, string viewPointID) {
        _surveyBuilder.SetQuestionViewPoint(questionID, viewPointID);
    }

    public void HandleAnswerAdded(int questionId, SurveyAnswerUIBase answerUI) {
        _surveyBuilder.AddNewAnswerToQuestion(questionId);
        if (answerUI is SurveyAnswerUIEditorString answerEditor) {
            answerEditor.OnTextChanged += HandleAnswerTextChanged;
        }
    }

    public void HandleAnswerOtherAdded(int questionId) {
        _surveyBuilder.AddNewAnswerToQuestion(questionId, true);
    }

    public void HandleAnswerTextChanged(int questionId, int answerId, string newText) {
        _surveyBuilder.SetAnswerText(questionId, answerId, newText);
    }

    public void HandleAnswerRemoved(int answerId) {
        _surveyBuilder.RemoveAnswer(answerId);
    }

    #endregion



    public void HandleSavePressed() {
        _surveyManager.SaveSurvey();
    }

    public void HandleExitPressed() {
        _surveyManager.ExitSurvey();
    }

    #endregion

    /// <summary>
    /// Builds the UI from data in the active survey
    /// </summary>
    public void DeserializeUI() {
        if (_surveyUIBuilder == null) _surveyUIBuilder = GetComponent<SurveyUIBuilder>();

        Survey survey = _surveyBuilder.GetActiveSurvey();
        // set title once we have the field

        AddQuestion addQuestion = GetComponent<AddQuestion>();
        addQuestion.SetInsertIndex(0);

        foreach (QuestionBase question in survey.GetAllQuestions()) {
            QuestionType questionType = question.QuestionType;
            SurveyQuestionUIBase questionUI = HandleExistingQuestionAdded(question, isDeserialized: true);
            questionUI.SetTitle(question.Title);
            questionUI.SetDescription(question.Description);
            if (question is QuestionGridBase gridQuestion) {
                if (questionUI is SurveyQuestionUIEditorGrid gridUI) {
                    for (int i = 0; i < gridQuestion.GetColumnCount(); i++) {
                        gridUI.AddExistingColumn(gridQuestion.GetColumn(i));
                    }
                    for (int i = 0; i < gridQuestion.GetRowCount(); i++) {
                        gridUI.AddExistingRow(gridQuestion.GetRow(i));
                    }
                }
            } else {
                foreach (AnswerBase answer in question.Answers) {
                    questionUI.AddAnswer(answer.Text, answer.IsOther);
                }
            }

            addQuestion.IncrementInsertIndex(1);
        }

        _surveyUIBuilder.RefreshAddQuestionBars();
    }
}
