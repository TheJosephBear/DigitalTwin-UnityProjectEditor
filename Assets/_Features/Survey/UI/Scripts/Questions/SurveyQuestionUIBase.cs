using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;
using System.Collections.Generic;
using System;

public abstract class SurveyQuestionUIBase {

    #region Fields & Properties

    public int QuestionID { get; }
    public string ImageID { get; set; }
    public QuestionType QuestionType => _questionType;

    protected VisualElement _root;
    protected SurveyUIBuilder _surveyUIBuilder;
    protected QuestionType _questionType;
    protected List<SerializableViewPoint> _viewPoints;

    protected List<SurveyAnswerUIBase> _addedAnswers = new();
    protected SurveyAnswerUIBase _otherAnswerUI;

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
        HideViewContainers();

     //   RegisterInputs(); // Called by ui builder instead after being added to the list
    }

    protected void HideViewContainers() {
        var cameraView = _root.Q<VisualElement>("camera-view");
        if (cameraView != null) {
            cameraView.style.display = DisplayStyle.None;
            cameraView.style.backgroundImage = null;
        }

        var questionImage = _root.Q<VisualElement>("question-image");
        if (questionImage != null) {
            questionImage.style.display = DisplayStyle.None;
            questionImage.style.backgroundImage = null;
        }
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

    public abstract SurveyAnswerUIBase AddAnswer(string answerText, bool isOther = false);
    public virtual void SetTitle(string title) {
        var field = _root.Q<TextField>("question-title");
        if (field != null) {
            field.SetValueWithoutNotify(title);
        }
    }

    public virtual void SetDescription(string desc) {
        var field = _root.Q<TextField>("question-description");
        if (field != null) {
            field.SetValueWithoutNotify(desc);
        }
    }

    public void SetQuestionPosition(int position) {
        var label = _root.Q<Label>("question-position");
        if (label != null) {
            label.text = position.ToString();
        }
    }

    public void SetQuestionType(string typeName) {
        var label = _root.Q<Label>("question-type");
        if (label != null) {
            label.text = typeName;
        }
    }

    public void SetRenderedImage(Texture texture) {
        _root.Q<VisualElement>("question-image").style.backgroundImage = Background.FromTexture2D((Texture2D)texture);
        _root.Q<VisualElement>("question-image").style.display = DisplayStyle.Flex;
    }

    public virtual void SetImageRender() {
        Debug.Log("Set image callled " + ImageID);
        var questionImage = _root.Q<VisualElement>("question-image");
        if (questionImage == null) return;

        if (string.IsNullOrEmpty(ImageID)) {
            questionImage.style.backgroundImage = null;
            questionImage.style.display = DisplayStyle.None;
            return;
        }

        TextureAsset textureAsset = ImageManager.Instance.GetTextureAssetByID(ImageID);
        if (textureAsset == null) {
            questionImage.style.backgroundImage = null;
            questionImage.style.display = DisplayStyle.None;
            return;
        }

        SetRenderedImage(textureAsset.Texture);
    }

    public void EnhanceImage(VisualElement sourceElement) {
        SurveyUIUtils.EnhanceImage(sourceElement, _surveyUIBuilder?.FullscreenImageOverlayTemplate);
    }

    #endregion

    #region UI Input Registration

    /// <summary>Registers all UI callbacks.</summary>
    public virtual void RegisterInputs() {
        RegisterTextInputs();
        RegisterButtons();
        RegisterDropdown();
    }

    protected abstract void RegisterTextInputs();

    protected abstract void RegisterButtons();

    protected abstract void RegisterDropdown();

    #endregion

    #region Answer Management



    protected virtual SurveyAnswerUIBase AddAnswerUI(bool isOther = false) {
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

    protected abstract SurveyAnswerUIBase CreateAnswerUI(
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
