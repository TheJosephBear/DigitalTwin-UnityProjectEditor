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


    // Question adding //
    private List<SurveyQuestionUI> _addedQuestions = new List<SurveyQuestionUI>();



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _scrollViewContent = _root.Q<ScrollView>("survey-scroll-view").contentContainer;
    }

    #region Input handling

    public void HandleQuestionAdded(string questionType) {
        VisualTreeAsset questionTemplate = questionTemplates.Find(b => b.name == questionType);
        TemplateContainer questionInstance = null;

        if (questionTemplate != null) {
            questionInstance = questionTemplate.Instantiate();
            _scrollViewContent.Add(questionInstance);
        } else {
            _scrollViewContent.Add(new Label($"Question template '{questionType}' is missing"));
        }

        QuestionType questionTypeEnum = QuestionTypeEnumToStringList.Find(a => a.StringValue == questionType).EnumValue;
        QuestionBase addedQuestion = SurveyBuilder.Instance.AddNewQuestion(questionTypeEnum);

        _addedQuestions.Add(
            new SurveyQuestionUI(
                questionInstance, 
                addedQuestion.Id,
                this,
                questionTypeEnum
            )
        );
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

    public void HandleAnswerTextChanged(AnswerBase answer, string newText) {
        SurveyBuilder.Instance.SetAnswerText(answer, newText);
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
