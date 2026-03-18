using System;
using System.Collections.Generic;
using SurveySystem;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Handles the Survey Builder UI logic. 
/// Receives input events from addedQuestion and answer fields, as well as buttons, 
/// and relays the changes to the <see cref="SurveyBuilder"/> for updating the survey data model.
/// (Tohle by možná mohl dìlat svùj vlastní script) -> Manages the instantiation of addedQuestion UI elements based on templates and keeps track of added questions.
/// </summary>
public class SurveyBuildingUI : MonoBehaviour {

    public List<QuestionTypeEnumStringCombination> QuestionTypeEnumToStringList = new List<QuestionTypeEnumStringCombination>();

    private SurveyBuildingManager _surveyBuildingManager;
    private SurveyBuilder _surveyBuilder;
    private VisualElement _root;
    private VisualElement _scrollViewContent;
    [SerializeField]
    private List<VisualTreeAsset> questionTemplates = new List<VisualTreeAsset>();
    [SerializeField]
    private VisualTreeAsset addQuestionBarTemplate;


    // Question adding //
    private List<SurveyQuestionUI> _addedQuestions = new List<SurveyQuestionUI>();
    private List<TemplateContainer> _addQuestionBars = new List<TemplateContainer>();



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



        // Add the initial bar at the start (before any questions)
        RefreshAddQuestionBars();
    }

    public void Initialize(SurveyBuilder surveyBuilder, SurveyBuildingManager manager) {
        _surveyBuilder = surveyBuilder;
        _surveyBuildingManager = manager;
        // This will also add all of the UI components according to the survey structure
    }

    #region UI building

    /// <summary>
    /// Rebuilds the scroll view content so there is one add-addedQuestion bar before each addedQuestion
    /// and one trailing bar after the last addedQuestion: [bar, q0, bar, q1, bar, ..., barN].
    /// </summary>
    private void RefreshAddQuestionBars() {
        if (addQuestionBarTemplate == null) {
            Debug.LogWarning("addQuestionBarTemplate is not assigned on SurveyBuildingUI.");
            return;
        }

        // Remove existing bars from the hierarchy
        foreach (var bar in _addQuestionBars) {
            bar.RemoveFromHierarchy();
        }
        _addQuestionBars.Clear();

        // Remove addedQuestion instances from hierarchy so we can re-insert them in order
        foreach (var question in _addedQuestions) {
            if (question.QuestionElement != null) {
                question.QuestionElement.RemoveFromHierarchy();
            }
        }

        // Re-insert: bar, [addedQuestion, bar] * N
        for (int i = 0; i <= _addedQuestions.Count; i++) {
            TemplateContainer bar = addQuestionBarTemplate.Instantiate();
            _addQuestionBars.Add(bar);
            _scrollViewContent.Add(bar);

            // Wire up the add-addedQuestion button on this bar
            int insertIndex = i; // capture for lambda
            TemplateContainer capturedBar = bar; // capture for lambda
            var addButton = bar.Q<Button>("add-question-button");
            if (addButton != null) {
                addButton.clicked += () => AddQuestion.Instance.OpenModalAtIndex(insertIndex, this, capturedBar);
            }

            if (i < _addedQuestions.Count && _addedQuestions[i].QuestionElement != null) {
                _scrollViewContent.Add(_addedQuestions[i].QuestionElement);
            }
        }
    }

    private void CreateAndInsertQuestion(VisualTreeAsset template, QuestionType questionType, int insertAtIndex) {
        TemplateContainer questionInstance;

        if (template != null) {
            questionInstance = template.Instantiate();
        } else {
            questionInstance = new TemplateContainer();
            questionInstance.Add(new Label($"Question template for '{questionType}' is missing"));
        }

        QuestionBase addedQuestion = _surveyBuilder.AddNewQuestion(questionType);

        var questionUI = new SurveyQuestionUI(
            questionInstance,
            addedQuestion.Id,
            this,
            questionType,
            FindAnyObjectByType<ViewManager>().GetSerializedViewPointsList()
        );

        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }

        RefreshAddQuestionBars();
    }

    #endregion

    #region Input handling

    public void HandleQuestionAdded(string questionTypeString, int insertAtIndex = -1) {
        VisualTreeAsset template = questionTemplates.Find(b => b.name == questionTypeString);

        QuestionType questionTypeEnum = QuestionTypeEnumToStringList
            .Find(a => a.StringValue == questionTypeString).EnumValue;

        HandleQuestionAdded(questionTypeEnum, insertAtIndex, template);
    }

    public void HandleQuestionAdded(QuestionType questionType, int insertAtIndex = -1, VisualTreeAsset template = null) {
        if(template == null) {
            // Template not provided, look it up
            var mappingEntry = QuestionTypeEnumToStringList
            .Find(a => a.EnumValue == questionType);

            if (mappingEntry == null) {
                Debug.LogError($"No mapping found for enum: {questionType}");
                return;
            }

            string templateName = mappingEntry.StringValue;
            template = questionTemplates.Find(b => b.name == templateName);
        }

        CreateAndInsertQuestion(template, questionType, insertAtIndex);
    }

    public void HandleQuestionDeleted(int questionIndex) {
        if (questionIndex < 0 || questionIndex >= _addedQuestions.Count) return;

        _surveyBuilder.RemoveQuestion(questionIndex);

        _addedQuestions[questionIndex].QuestionElement?.RemoveFromHierarchy();
        _addedQuestions.RemoveAt(questionIndex);

        RefreshAddQuestionBars();
    }

    public void HandleQuestionMoved(int questionIndex, int direction) {
        int targetIndex = questionIndex + direction;
        if (targetIndex < 0 || targetIndex >= _addedQuestions.Count) return;

        var temp = _addedQuestions[questionIndex];
        _addedQuestions[questionIndex] = _addedQuestions[targetIndex];
        _addedQuestions[targetIndex] = temp;

        RefreshAddQuestionBars();
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

    public void HandleAnswerAdded(int questionId) {
        _surveyBuilder.AddNewAnswerToQuestion(questionId);
    }

    public void HandleAnswerOtherAdded(int questionId) {
        _surveyBuilder.AddNewAnswerToQuestion(questionId, true);
    }

    public void HandleAnswerTextChanged(AnswerBase answer, string newText) {
        _surveyBuilder.SetAnswerText(answer, newText);
    }

    public void HandleAnswerTextChanged(int questionId, int answerId, string newText) {
        _surveyBuilder.SetAnswerText(questionId, answerId, newText);
    }

    public void HandleAnswerRemoved(AnswerBase answer) {
        _surveyBuilder.RemoveAnswer(answer.Idx);
    }

    public void HandleSavePressed() {
        _surveyBuildingManager.SaveSurvey();
    }

    public void HandleExitPressed() {
        _surveyBuildingManager.ExitSurveyCreation();
    }

    #endregion


    /// <summary>Returns the current index of the given addedQuestion in the list, or -1 if not found.</summary>
    public int GetQuestionIndex(SurveyQuestionUI questionUI) {
        return _addedQuestions.IndexOf(questionUI);
    }

    SurveyQuestionUI AddQuestionDeserialization(QuestionBase addedQuestion) {
        QuestionType questionType = addedQuestion.QuestionType;
        int insertAtIndex = -1;
        var mappingEntry = QuestionTypeEnumToStringList
            .Find(a => a.EnumValue == questionType);

        if (mappingEntry == null) {
            Debug.LogError($"No mapping found for enum: {questionType}");
            return null;
        }

        string templateName = mappingEntry.StringValue;

        VisualTreeAsset template = questionTemplates
            .Find(b => b.name == templateName);

        TemplateContainer questionInstance;

        if (template != null) {
            questionInstance = template.Instantiate();
        } else {
            questionInstance = new TemplateContainer();
            questionInstance.Add(new Label($"Question template for '{questionType}' is missing"));
        }

        var questionUI = new SurveyQuestionUI(
            questionInstance,
            addedQuestion.Id,
            this,
            questionType,
            FindAnyObjectByType<ViewManager>().GetSerializedViewPointsList(),
            isDeserialized: true
        );

        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }

        return questionUI;
    }

    public void DeserializeUI() {
        Survey survey = _surveyBuilder.GetActiveSurvey();
        // set title once we have the field

        foreach (QuestionBase question in survey.GetAllQuestions()) {
            SurveyQuestionUI questionUI =  AddQuestionDeserialization(question);
            questionUI.SetTitle(question.Title);
            questionUI.SetDescription(question.Description);
            foreach (AnswerBase answer in question.Answers) {
                questionUI.AddAnswer(answer.Text, answer.IsOther);
            }
        }

        RefreshAddQuestionBars();
    }
}

[Serializable]
public class QuestionTypeEnumStringCombination {
    public QuestionType EnumValue;
    public string StringValue;
}
