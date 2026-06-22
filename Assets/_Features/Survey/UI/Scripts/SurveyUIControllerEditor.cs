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

        RegisterBaseSurveyInputs();
    }

    void RegisterBaseSurveyInputs() {
        var titleField = _root.Q<TextField>("question-title");
        var descField = _root.Q<TextField>("question-description");

        if (titleField == null || descField == null) {
            print("Title or description field not found.");
            return;
        }

        titleField.RegisterValueChangedCallback(evt => {
            _surveyBuilder.SetSurveyName(evt.newValue);
        });

        descField.RegisterValueChangedCallback(evt => {
            _surveyBuilder.SetSurveyDescription(evt.newValue);
        });
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
        //    if (questionTypeEnum == QuestionType.ImageChoice) questionTypeEnum = QuestionType.MultipleChoiceSingle; // IMAGE CHOICE ZATÍM NEDÁVAT
        HandleQuestionAdded(questionTypeEnum, insertAtIndex);
    }

    public SurveyQuestionUIBase HandleQuestionAdded(QuestionType questionType, int insertAtIndex = -1) {
        QuestionBase newQuestion = _surveyBuilder.AddNewQuestion(questionType);
        return HandleExistingQuestionAdded(newQuestion, insertAtIndex);
    }

    public SurveyQuestionUIBase HandleExistingQuestionAdded(QuestionBase question, int insertAtIndex = -1, bool isDeserialized = false) {
        SurveyQuestionUIBase addedQuestionUI = _surveyUIBuilder.AddQuestionEditor(question, insertAtIndex: insertAtIndex, isDeserialized: isDeserialized);

        if (addedQuestionUI is SurveyQuestionUIEditor editorUI) {
            // Use a lambda to fetch the LATEST index from the UI list at the moment the event fires
            editorUI.OnQuestionDeleted += (id) => {
                int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(addedQuestionUI);
                HandleQuestionDeleted(dynamicIdx);
            };

            editorUI.OnQuestionMoved += (id, direction) => {
                int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(addedQuestionUI);
                HandleQuestionMoved(dynamicIdx, direction);
            };

            editorUI.OnTitleChanged += HandleQuestionTitleChanged;
            editorUI.OnDescriptionChanged += HandleQuestionDescriptionChanged;
       //     editorUI.OnQuestionDeleted += HandleQuestionDeleted;
      //      editorUI.OnQuestionMoved += HandleQuestionMoved;
            editorUI.OnUploadImage += HandleImageUpload;
            editorUI.OnViewpointSelected += HandleQuestionViewPointSelected;
            editorUI.OnMoveAnswer += HandleAnswerMoved;
            editorUI.OnToggleRequired += HandleRequiredChange;
        }

        if (addedQuestionUI is SurveyQuestionUIEditorGrid gridUI) {
            gridUI.OnAddRow += AddRow;
            gridUI.OnAddColumn += AddColumn;
            // Temporary - initial answer adding
            if (!isDeserialized) {
                AddRow(gridUI.QuestionID, gridUI.AddRow());
                AddColumn(gridUI.QuestionID, gridUI.AddColumn());
            }
        } else if (addedQuestionUI is SurveyQuestionUIEditorString builderUI) {
            builderUI.OnAnswerAdded += HandleAnswerAdded;
            builderUI.OnAnswerOtherAdded += HandleAnswerOtherAdded;
            builderUI.OnAnswerRemoved += HandleAnswerRemoved;

            if (!isDeserialized) {
                builderUI.AddInitialAnswer();
            }
        } else if (addedQuestionUI is SurveyQuestionUIEditorImage imageUI) {
            imageUI.OnAnswerImageChanged += HandleImageQuestionAnswerImageUpload;
            imageUI.OnAnswerAdded += HandleAddAnswerImage;
            imageUI.OnAnswerRemoved += HandleAnswerRemoved;
        }

        return addedQuestionUI;
    }

    void HandleRequiredChange(int questionID, bool isRequired) {
        _surveyBuilder.SetQuestionRequired(questionID, isRequired);
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
        print($"(Controller) Deleting index {questionIndex}");
        if (!_surveyUIBuilder.DeleteQuestion(questionIndex)) return;
        _surveyBuilder.RemoveQuestion(questionIndex);
    }

    public void HandleQuestionMoved(int questionIndex, int direction) {
        print($"(Controller) Moving index {questionIndex} in direction: {direction}");
        _surveyUIBuilder.MoveQuestion(questionIndex, direction); 
        _surveyBuilder.MoveQuestion(questionIndex, direction);
    }

    void HandleAnswerMoved(int questionIndex, int answerIndex, int direction) {
        _surveyBuilder.MoveAnswer(questionIndex, answerIndex, direction);
    }

    public void HandleQuestionTitleChanged(int questionId, string newText) {
        _surveyBuilder.SetQuestionTitle(questionId, newText);
    }

    public void HandleQuestionDescriptionChanged(int questionId, string newText) {
        _surveyBuilder.SetQuestionDescription(questionId, newText);
    }

    public void HandleQuestionViewPointSelected(int questionID, string viewPointID) {
        _surveyBuilder.SetQuestionViewPointID(questionID, viewPointID);
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

    public void HandleAnswerRemoved(int questionId, int answerId) {
        _surveyBuilder.RemoveAnswer(questionId, answerId);
    }

    void HandleImageUpload(int questionIndex) {
        ImageManager.Instance.AskForImageDialog((textureAsset) => {
            // Validation: Ensure the asset and ID actually exist
            if (textureAsset == null || string.IsNullOrEmpty(textureAsset.ID)) {
                Debug.LogError("Upload failed: TextureAsset or ID is null");
                return;
            }

            Debug.Log($"Assigning ImageID: {textureAsset.ID} to question: {questionIndex}");

            // Update the Data Model
            _surveyBuilder.SetQuestionImageID(questionIndex, textureAsset.ID);

            // Update the UI
            _surveyUIBuilder.SetQuestionImage(questionIndex, textureAsset.ID);
        });
    }

    void HandleImageQuestionAnswerImageUpload(int questionID, int answerID, string imageID) {
        _surveyBuilder.SetAnswerImage(questionID, answerID, imageID);
    }

    void HandleAddAnswerImage(int questionID) {
        _surveyBuilder.AddNewAnswerToQuestion(questionID);
    }

    #endregion



    public void HandleSavePressed() {
        _surveyManager.SaveSurvey();
    }

    public void HandleExitPressed() {
        PopUp.Instance.AreYouSurePopUp((exit) => {
            if (exit) {
                _surveyManager.ExitSurvey();
            }
        });
    }

    #endregion

    /// <summary>
    /// Builds the UI from data in the active survey
    /// </summary>
    public void DeserializeUI() {
        if (_surveyUIBuilder == null) _surveyUIBuilder = GetComponent<SurveyUIBuilder>();

        Survey survey = _surveyBuilder.GetActiveSurvey();
        // set title
        var titleField = _root.Q<TextField>("question-title");
        var descField = _root.Q<TextField>("question-description");
        titleField.value = survey.Name;
        descField.value = survey.Description;

        _surveyUIBuilder.ClearScrollviewContent();

        foreach (QuestionBase question in survey.GetAllQuestions()) {
            QuestionType questionType = question.QuestionType;
            SurveyQuestionUIBase questionUI = HandleExistingQuestionAdded(question, isDeserialized: true);
            questionUI.SetTitle(question.Title);
            questionUI.SetDescription(question.Description);
            questionUI.ImageID = question.ImageID;

            print("Calling required: " + question.IsRequired);
            (questionUI as SurveyQuestionUIEditor).SetRequired(question.IsRequired);
            // Set selected viewpoint
            if (MainManagerBase.Instance != null) {
                ViewPoint vp = MainManagerBase.Instance.ViewManager.GetViewPointByID(question.ViewPointId);
                if (vp != null) {
                    (questionUI as SurveyQuestionUIEditor).SetSelectedView(vp);
                } else {
                    questionUI.SetImageRender();
                }
            }

            if (question is QuestionGridBase gridQuestion) {
                if (questionUI is SurveyQuestionUIEditorGrid gridUI) {
                    for (int i = 0; i < gridQuestion.GetColumnCount(); i++) {
                        gridUI.AddExistingColumn(gridQuestion.GetColumn(i));
                    }
                    for (int i = 0; i < gridQuestion.GetRowCount(); i++) {
                        gridUI.AddExistingRow(gridQuestion.GetRow(i));
                    }
                }
            } else if (question is QuestionImageChoice imageQuestion) {
                if (questionUI is SurveyQuestionUIEditorImage imageUI) {
                    foreach (AnswerImage answer in imageQuestion.Answers) {
                        imageUI.AddAnswerWithImage(answer.ImageID);
                    }
                }
            } else if (questionUI is SurveyQuestionUIEditorString stringQuestion) {
                foreach (AnswerBase answer in question.Answers) {
                    SurveyAnswerUIBase answerUIBase = questionUI.AddAnswer(answer.Text, answer.IsOther);
                    if(answerUIBase is SurveyAnswerUIEditorString answerEditorString)
                        answerEditorString.OnTextChanged += HandleAnswerTextChanged;
                }
            }
        }

        _surveyUIBuilder.RefreshAddQuestionBars();
    }
}
