using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UIRadioButton = UnityEngine.UIElements.RadioButton;

public class SurveyQuestionUIEditorString : SurveyQuestionUIEditor {

    #region Events

    public event Action<int, SurveyAnswerUIBase> OnAnswerAdded;
    public event Action<int, SurveyAnswerUIBase> OnAnswerOtherAdded;
    public event Action<int, int> OnAnswerRemoved;
    public event Action<int, int, string> OnAnswerTextChanged;
    public event Action<int, int> OnMaxAnswerLengthChanged; // questionId, max length (0 = unlimited)

    #endregion

    private const int DefaultMaxAnswerLength = 200;
    private const int MaxAllowedAnswerLength = 10000;

    private VisualElement _maxLengthContainer;
    private Toggle _maxLengthToggle;
    private IntegerField _maxLengthField;
    private VisualElement _maxLengthHint;

    private bool IsTextQuestion => _questionType == QuestionType.Paragraph || _questionType == QuestionType.ShortAnswer;

    public SurveyQuestionUIEditorString(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder,
        bool isDeserialized)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return new SurveyAnswerUIEditorString(element, index, this, isOther);
    }

    #region Init

    protected override void RegisterButtons() {
        base.RegisterButtons();
        var addOptionButton = _root.Q<Button>("add-option-button");
        if (addOptionButton != null) {
            addOptionButton.clicked += () =>
                OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
        } else {
            Debug.LogWarning("[RegisterButtons] add-option-button not found");
        }

        var addOtherButton = _root.Q<Button>("add-other-option-button");
        if (addOtherButton != null) {
            addOtherButton.clicked += () => {
                if (_otherAnswerUI == null) {
                    var otherUI = AddAnswerUI(true);
                    OnAnswerOtherAdded?.Invoke(QuestionID, otherUI);

                    // Set after the model answer exists so the change callback stores the text
                    TextField otherTextField = otherUI?.AnswerElement != null ? FindTextFieldRecursive(otherUI.AnswerElement) : null;
                    if (otherTextField != null)
                        otherTextField.value = "Jiná";

                    UpdateMaxLengthVisibility();
                }
            };
        } else {
            Debug.LogWarning("[RegisterButtons] add-other-option-button not found");
        }

        RegisterQuestionModalButtonEvents();
        RegisterMaxLengthInputs();
        SetupAnswerPreview();
    }

    /// <summary>
    /// The disabled answer preview ignores picking so hover lands on its wrapper,
    /// which shows the tooltip and keeps the default cursor instead of the text I-beam.
    /// </summary>
    private void SetupAnswerPreview() {
        var previewField = _root.Q<TextField>("answer-preview-field");
        if (previewField == null) return;

        previewField.pickingMode = PickingMode.Ignore;
        previewField.Query<VisualElement>().ForEach(child => child.pickingMode = PickingMode.Ignore);
    }

    private void RegisterMaxLengthInputs() {
        _maxLengthContainer = _root.Q<VisualElement>("max-length-container");
        _maxLengthToggle = _root.Q<Toggle>("max-length-toggle");
        _maxLengthField = _root.Q<IntegerField>("max-length-field");
        _maxLengthHint = _root.Q<VisualElement>("max-length-hint");

        if (_maxLengthContainer == null || _maxLengthToggle == null || _maxLengthField == null) return;

        // Commit on Enter / focus loss so clearing the field to retype doesn't snap it to 1
        _maxLengthField.isDelayed = true;

        _maxLengthToggle.RegisterValueChangedCallback(evt => {
            UpdateMaxLengthFieldDisplay();
            NotifyMaxAnswerLengthChanged();
        });

        _root.Q<Label>("max-length-label")?.RegisterCallback<ClickEvent>(evt => {
            _maxLengthToggle.value = !_maxLengthToggle.value;
        });

        _maxLengthField.RegisterValueChangedCallback(evt => {
            int clamped = Mathf.Clamp(evt.newValue, 1, MaxAllowedAnswerLength);
            if (clamped != evt.newValue) {
                _maxLengthField.SetValueWithoutNotify(clamped);
            }
            NotifyMaxAnswerLengthChanged();
        });

        UpdateMaxLengthFieldDisplay();
        UpdateMaxLengthVisibility();
    }

    #endregion

    #region Max Answer Length

    /// <summary>Restores the limit from the data model (0 = unlimited) without firing change events.</summary>
    public void SetMaxAnswerLength(int maxLength) {
        if (_maxLengthToggle == null || _maxLengthField == null) return;

        bool isLimited = maxLength > 0;
        _maxLengthToggle.SetValueWithoutNotify(isLimited);
        _maxLengthField.SetValueWithoutNotify(isLimited ? Mathf.Min(maxLength, MaxAllowedAnswerLength) : DefaultMaxAnswerLength);
        UpdateMaxLengthFieldDisplay();
    }

    private void NotifyMaxAnswerLengthChanged() {
        int maxLength = _maxLengthToggle.value ? _maxLengthField.value : 0;
        OnMaxAnswerLengthChanged?.Invoke(QuestionID, maxLength);
    }

    private void UpdateMaxLengthFieldDisplay() {
        if (_maxLengthToggle == null) return;

        DisplayStyle display = _maxLengthToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
        _maxLengthField.style.display = display;
        if (_maxLengthHint != null) _maxLengthHint.style.display = display;
    }

    /// <summary>Choice questions only offer the limit once they have an "Other" option to type into.</summary>
    private void UpdateMaxLengthVisibility() {
        if (_maxLengthContainer == null) return;

        bool hasFreeText = IsTextQuestion || _otherAnswerUI != null;
        _maxLengthContainer.style.display = hasFreeText ? DisplayStyle.Flex : DisplayStyle.None;
    }

    #endregion

    #region Answer Management

    public void AddInitialAnswer() {
        OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
    }

    public override SurveyAnswerUIBase AddAnswer(string answerText, bool isOther = false) {
        SurveyAnswerUIBase answerUI = base.AddAnswer(answerText, isOther);
        if (isOther) UpdateMaxLengthVisibility();
        return answerUI;
    }

    public void SelectAnswerRadio(int selectedIndex) {
        if (_questionType != QuestionType.MultipleChoiceSingle) return;

        for (int i = 0; i < _addedAnswers.Count; i++) {
            if (_addedAnswers[i]?.AnswerElement != null) {
                var customRadio = _addedAnswers[i].AnswerElement.Q<CustomRadioButton>();
                if (customRadio != null && customRadio.Radio != null) {
                    customRadio.Radio.SetValueWithoutNotify(i == selectedIndex);
                }
            }
        }

        if (_otherAnswerUI?.AnswerElement != null) {
            var customRadio = _otherAnswerUI.AnswerElement.Q<CustomRadioButton>();
            if (customRadio != null && customRadio.Radio != null) {
                customRadio.Radio.SetValueWithoutNotify(_otherAnswerUI.AnswerIndex == selectedIndex);
            }
        }
    }

    /// <summary>Deletes an answer.</summary>
    public void DeleteAnswer(int index) {
        if (index < 0) return;

        if (_otherAnswerUI != null && index == _otherAnswerUI.AnswerIndex) {
            _optionsList.Remove(_otherAnswerUI.AnswerElement);
            _otherAnswerUI = null;
            UpdateMaxLengthVisibility();
            OnAnswerRemoved?.Invoke(QuestionID, index);
            return;
        }

        if (index >= _addedAnswers.Count) return;

        var answer = _addedAnswers[index];

        _optionsList.Remove(answer.AnswerElement);
        _addedAnswers.RemoveAt(index);

        RecalculateAnswerIndices();

        OnAnswerRemoved?.Invoke(QuestionID, index);
    }

    #endregion

}
