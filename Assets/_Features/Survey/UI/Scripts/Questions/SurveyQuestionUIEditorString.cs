using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIEditorString : SurveyQuestionUIEditor {

    #region Events

    public event Action<int, SurveyAnswerUIEditorString> OnAnswerAdded;
    public event Action<int> OnAnswerOtherAdded;
    public event Action<int> OnAnswerRemoved;
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

    protected override SurveyAnswerUIEditorString CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return new SurveyAnswerUIEditorString(element, index, this, isOther);
    }

    #region Init

    protected override void RegisterButtons() {
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
                    OnAnswerOtherAdded?.Invoke(QuestionID);
                    AddAnswerUI(true);
                }
            };
        } else {
            Debug.LogWarning("[RegisterButtons] add-other-option-button not found");
        }

        var editButton = _root.Q<VisualElement>("edit-question-button");
        if (editButton != null) {
            editButton.RegisterCallback<ClickEvent>(OnEditQuestionClicked);
        } else {
            Debug.LogWarning("[RegisterButtons] edit-question-button not found");
        }
    }

    #endregion

    #region Answer Management

    public void AddInitialAnswer() {
        OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
    }

    /// <summary>Deletes an answer.</summary>
    public void DeleteAnswer(int index) {
        if (index < 0) return;

        if (_otherAnswerUI != null && index == _otherAnswerUI.AnswerIndex) {
            _optionsList.Remove(_otherAnswerUI.AnswerElement);
            _otherAnswerUI = null;
            OnAnswerRemoved?.Invoke(index);
            return;
        }

        if (index >= _addedAnswers.Count) return;

        var answer = _addedAnswers[index];

        _optionsList.Remove(answer.AnswerElement);
        _addedAnswers.RemoveAt(index);

        RecalculateAnswerIndices();

        OnAnswerRemoved?.Invoke(index);
    }

    #endregion
}
