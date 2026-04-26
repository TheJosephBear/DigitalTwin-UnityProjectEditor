using SurveySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyUIBuilder : MonoBehaviour {

    public QuestionUIMapping questionUIMapping;

    [SerializeField]
    private VisualTreeAsset addQuestionBarTemplate;

    private VisualElement _root;
    private VisualElement _scrollViewContent;

    // Question adding //
    private List<SurveyQuestionUIBase> _addedQuestions = new List<SurveyQuestionUIBase>();
    private List<TemplateContainer> _addQuestionBars = new List<TemplateContainer>();

    private List<QuestionType> QuestionTypesUsingStringUI = new List<QuestionType>{
        QuestionType.MultipleChoiceSingle,
        QuestionType.MultipleChoiceMultiple,
        QuestionType.Paragraph,
        QuestionType.ShortAnswer,
        QuestionType.LinearScale,
    };

    private List<QuestionType> QuestionTypesUsingGridUI = new List<QuestionType>{
        QuestionType.MultipleChoiceGrid,
        QuestionType.CheckboxGrid
    };

    void Awake() {
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _scrollViewContent = _root.Q<ScrollView>("survey-scroll-view").contentContainer;

        // Add the initial bar at the start (before any questions)
        RefreshAddQuestionBars();
    }

    /// <summary>
    /// Rebuilds the scroll view content so there is one add-addedQuestion bar before each addedQuestion
    /// and one trailing bar after the last addedQuestion: [bar, q0, bar, q1, bar, ..., barN].
    /// </summary>
    public void RefreshAddQuestionBars() {
        // Survey Viewing
        /*
        if (_isViewerUI) {
            for (int i = 0; i <= _addedQuestions.Count; i++) {
                if (i < _addedQuestions.Count && _addedQuestions[i].QuestionElement != null) {
                    _scrollViewContent.Add(_addedQuestions[i].QuestionElement);
                }
            }
            return;
        }
        */

        // Survey Building

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
                addButton.clicked += () => AddQuestion.Instance.OpenModalAtIndex(insertIndex, capturedBar);
            }

            if (i < _addedQuestions.Count && _addedQuestions[i].QuestionElement != null) {
                _scrollViewContent.Add(_addedQuestions[i].QuestionElement);
            }
        }
    }

    public SurveyQuestionUIBase AddQuestionEditor(QuestionBase addedQuestion, bool isDeserialized, int insertAtIndex = -1, VisualTreeAsset template = null) {
        print("Adding question in builder, type is: " + addedQuestion.QuestionType);
        if (template == null) {
            //Template not provided, look it up
            QuestionTypeMapping mapping = questionUIMapping.GetMappingByQuestionType(addedQuestion.QuestionType);

            if (mapping == null) {
                Debug.LogError($"No mapping found for enum: {addedQuestion.QuestionType}");
                return null;
            }

            template = mapping.QuestionTemplate;
        }

        return CreateQuestion(addedQuestion, template, insertAtIndex: insertAtIndex, isDeserialized: isDeserialized);
    }

    private SurveyQuestionUIBase CreateQuestion(QuestionBase addedQuestion, VisualTreeAsset template, bool isDeserialized, int insertAtIndex = -1) {
        QuestionType questionType = addedQuestion.QuestionType;
        TemplateContainer questionInstance;

        if (template != null) {
            questionInstance = template.Instantiate();
        } else {
            questionInstance = new TemplateContainer();
            questionInstance.Add(new Label($"Question template for '{questionType}' is missing"));
        }


        // The decision logic needs to get better this is horrible
        SurveyQuestionUIBase questionUI = null;

        if(QuestionTypesUsingStringUI.Contains(questionType)) {
            questionUI = new SurveyQuestionUIEditorString(
                questionInstance,
                addedQuestion.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this,
                isDeserialized: isDeserialized
            );
        } else if(QuestionTypesUsingGridUI.Contains(questionType)) {
            questionUI = new SurveyQuestionUIEditorGrid(
                questionInstance,
                addedQuestion.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this,
                isDeserialized: isDeserialized
            );
        }

        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }

        RefreshAddQuestionBars();
        return questionUI;
    }
    /*
    public ISurveyQuestionUI AddQuestionViewer(QuestionBase questionBase, int insertAtIndex = -1) {
        QuestionType questionType = questionBase.QuestionType;
        TemplateContainer questionInstance;

        QuestionTypeMapping mapping = questionUIMapping.GetMappingByQuestionType(questionType);

        if (mapping == null) {
            Debug.LogError($"No mapping found for enum: {questionType}");
            return null;
        }

        VisualTreeAsset template = mapping.QuestionTemplate;

        questionInstance = template != null
            ? template.Instantiate()
            : new TemplateContainer();

        if (template == null) {
            questionInstance.Add(new Label($"Missing template for '{questionType}'"));
        }

        ISurveyQuestionUI questionUI = new SurveyQuestionUIViewer(
            questionInstance,
            questionBase.Id,
            questionType,
            FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
            this
        );

        _scrollViewContent.Add(questionInstance);

        foreach (var bar in _addQuestionBars) {
            bar.RemoveFromHierarchy();
        }
        _addQuestionBars.Clear();


        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }

        return questionUI;
    }
    */
    public void MoveQuestion(int questionIndex, int direction) {
        int targetIndex = questionIndex + direction;
        if (targetIndex < 0 || targetIndex >= _addedQuestions.Count) return;

        var temp = _addedQuestions[questionIndex];
        _addedQuestions[questionIndex] = _addedQuestions[targetIndex];
        _addedQuestions[targetIndex] = temp;

        RefreshAddQuestionBars();
    }

    public bool DeleteQuestion(int questionIndex) {
        if (questionIndex < 0 || questionIndex >= _addedQuestions.Count) return false;

        _addedQuestions[questionIndex].QuestionElement?.RemoveFromHierarchy();
        _addedQuestions.RemoveAt(questionIndex);

        RefreshAddQuestionBars();
        return true;
    }

    /// <summary>Returns the current index of the given addedQuestion in the list, or -1 if not found.</summary>
    public int GetQuestionIndex(SurveyQuestionUIBase questionUI) {
        return _addedQuestions.IndexOf(questionUI);
    }

    public void ClearScrollviewContent() {
        foreach (var question in _addedQuestions) {
            if (question.QuestionElement != null) {
                question.QuestionElement.RemoveFromHierarchy();
            }
        }
    }
}
