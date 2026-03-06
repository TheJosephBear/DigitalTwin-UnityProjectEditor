using System;
using System.Collections.Generic;
using SurveySystem;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Handles the Survey Builder UI logic. 
/// Receives input events from question and answer fields, as well as buttons, 
/// and relays the changes to the <see cref="SurveyBuilder"/> for updating the survey data model.
/// (Tohle by možná mohl dìlat svùj vlastní script) -> Manages the instantiation of question UI elements based on templates and keeps track of added questions.
/// </summary>
public class SurveyBuildingUI : MonoBehaviour {

    public List<QuestionTypeEnumStringCombination> QuestionTypeEnumToStringList = new List<QuestionTypeEnumStringCombination>();

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

        // Add the initial bar at the start (before any questions)
        RefreshAddQuestionBars();
    }

    #region Input handling

    public void HandleQuestionAdded(string questionType, int insertAtIndex = -1) {
        VisualTreeAsset questionTemplate = questionTemplates.Find(b => b.name == questionType);
        TemplateContainer questionInstance = null;

        if (questionTemplate != null) {
            questionInstance = questionTemplate.Instantiate();
        } else {
            questionInstance = new TemplateContainer();
            questionInstance.Add(new Label($"Question template '{questionType}' is missing"));
        }

        QuestionType questionTypeEnum = QuestionTypeEnumToStringList.Find(a => a.StringValue == questionType).EnumValue;
        QuestionBase addedQuestion = SurveyBuilder.Instance.AddNewQuestion(questionTypeEnum);

        var questionUI = new SurveyQuestionUI(
            questionInstance,
            addedQuestion.Id,
            this,
            questionTypeEnum
        );

        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }

        RefreshAddQuestionBars();
    }

    /// <summary>Returns the current index of the given question in the list, or -1 if not found.</summary>
    public int GetQuestionIndex(SurveyQuestionUI questionUI) {
        return _addedQuestions.IndexOf(questionUI);
    }

    public void HandleQuestionDeleted(int questionIndex) {
        if (questionIndex < 0 || questionIndex >= _addedQuestions.Count) return;

        SurveyBuilder.Instance.RemoveQuestion(questionIndex);

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

    /// <summary>
    /// Rebuilds the scroll view content so there is one add-question bar before each question
    /// and one trailing bar after the last question: [bar, q0, bar, q1, bar, ..., barN].
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

        // Remove question instances from hierarchy so we can re-insert them in order
        foreach (var question in _addedQuestions) {
            if (question.QuestionElement != null) {
                question.QuestionElement.RemoveFromHierarchy();
            }
        }

        // Re-insert: bar, [question, bar] * N
        for (int i = 0; i <= _addedQuestions.Count; i++) {
            TemplateContainer bar = addQuestionBarTemplate.Instantiate();
            _addQuestionBars.Add(bar);
            _scrollViewContent.Add(bar);

            // Wire up the add-question button on this bar
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

    public void HandleQuestionTitleChanged(int questionId, string newText) {
        SurveyBuilder.Instance.SetQuestionTitle(questionId, newText);
    }

    public void HandleQuestionDescriptionChanged(int questionId, string newText) {
        SurveyBuilder.Instance.SetQuestionDescription(questionId, newText);
    }

    public void HandleAnswerAdded(int questionId) {
        SurveyBuilder.Instance.AddNewAnswerToQuestion(questionId);
    }

    public void HandleAnswerOtherAdded(int questionId) {
        SurveyBuilder.Instance.AddNewAnswerToQuestion(questionId, true);
    }

    public void HandleAnswerTextChanged(AnswerBase answer, string newText) {
        SurveyBuilder.Instance.SetAnswerText(answer, newText);
    }

    public void HandleAnswerTextChanged(int questionId, int answerId, string newText) {
        SurveyBuilder.Instance.SetAnswerText(questionId, answerId, newText);
    }

    public void HandleAnswerRemoved(AnswerBase answer) {
        SurveyBuilder.Instance.RemoveAnswer(answer.Idx);
    }

    #endregion
}

[Serializable]
public class QuestionTypeEnumStringCombination {
    public QuestionType EnumValue;
    public string StringValue;
}
