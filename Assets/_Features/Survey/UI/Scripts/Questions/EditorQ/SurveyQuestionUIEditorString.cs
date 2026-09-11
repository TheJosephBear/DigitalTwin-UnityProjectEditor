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

    #endregion

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
                }
            };
        } else {
            Debug.LogWarning("[RegisterButtons] add-other-option-button not found");
        }

        RegisterQuestionModalButtonEvents();
    }

    #endregion

    #region Answer Management

    public void AddInitialAnswer() {
        OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
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
