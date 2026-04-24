using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;
using System.Collections.Generic;
using System;

public abstract class SurveyQuestionUIBase : MonoBehaviour {

    #region Fields & Properties

    public int QuestionID { get; }

    protected VisualElement _root;
    protected SurveyUIBuilder _surveyUIBuilder;
    protected QuestionType _questionType;
    protected List<SerializableViewPoint> _viewPoints;

    protected List<SurveyAnswerUIEditorString> _addedAnswers = new();
    protected SurveyAnswerUIEditorString _otherAnswerUI;

    protected VisualElement _optionsList;
    protected VisualTreeAsset _answerTemplate;

    public VisualElement QuestionElement => _root;

    #endregion


    public SurveyQuestionUIBase(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder) {

        _root = root;
        QuestionID = questionId;
        _questionType = questionType;
        _viewPoints = viewPoints;
        _surveyUIBuilder = uiBuilder;

        LoadAnswerTemplate();
        InitializeOptionsList();

        RegisterInputs();
    }

    #region Initialization

    /// <summary>Loads the correct answer template based on question type.</summary>
    protected void LoadAnswerTemplate() {
        var mapping = _surveyUIBuilder.questionUIMapping;

        if (mapping == null) {
            Debug.LogError("QuestionUIMapping not found!");
            return;
        }

        _answerTemplate = mapping.GetAnswerUITemplate(_questionType);

        if (_answerTemplate == null)
            Debug.LogWarning($"No template for {_questionType}");
    }

    /// <summary>Finds and prepares the options container.</summary>
    protected void InitializeOptionsList() {
        if (_root == null) return;

        _optionsList = _root.Q<RadioButtonGroup>("options-list") ??
                       _root.Q<VisualElement>("options-list");

        _optionsList?.Clear();
    }

    #endregion

    #region Interface for editing the question

    public abstract void AddAnswer(string answerText, bool isOther = false);
    public virtual void SetTitle(string title) {
        _root.Q<TextField>("question-title").value = title;
    }

    public virtual void SetDescription(string desc) {
        _root.Q<TextField>("question-description").value = desc;
    }

    #endregion

    #region UI Input Registration

    /// <summary>Registers all UI callbacks.</summary>
    protected virtual void RegisterInputs() {
        RegisterTextInputs();
        RegisterButtons();
        RegisterDropdown();
    }

    protected abstract void RegisterTextInputs();

    protected abstract void RegisterButtons();

    protected abstract void RegisterDropdown();

    #endregion

    #region Answer Management



    protected virtual SurveyAnswerUIEditorString AddAnswerUI(bool isOther = false) {
        if (_optionsList == null || _answerTemplate == null)
            return null;

        var element = _answerTemplate.Instantiate();
        int index = _addedAnswers.Count;

        if (isOther) {
            _optionsList.Add(element);

            var radio = element.Q<CustomRadioButton>();
            if (radio != null) {
                radio.Placeholder = "Other";
            }

            _otherAnswerUI = CreateAnswerUI(element, index, true);
            return _otherAnswerUI;
        }

        InsertAnswerElement(element);

        var answerUI = CreateAnswerUI(element, index, false);

        _addedAnswers.Add(answerUI);

        return answerUI;
    }

    /// <summary>Inserts answer before "Other" if it exists.</summary>
    protected void InsertAnswerElement(VisualElement element) {
        if (_otherAnswerUI != null) {
            int idx = _optionsList.IndexOf(_otherAnswerUI.AnswerElement);
            _optionsList.Insert(idx, element);
        } else {
            _optionsList.Add(element);
        }
    }

    protected abstract SurveyAnswerUIEditorString CreateAnswerUI(
        VisualElement element,
        int index,
        bool isOther
    ); // return new MyCustomSurveyAnswerUI(element, index, this, isOther);

    #endregion

    #region Helpers

    protected TextField FindTextFieldRecursive(VisualElement root) {
        if (root is TextField tf) return tf;

        foreach (var child in root.Children()) {
            var result = FindTextFieldRecursive(child);
            if (result != null) return result;
        }

        return null;
    }

    #endregion


}