using SurveySystem;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIViewerString : SurveyQuestionUIViewer {

    #region Events
    public event Action<int, int, bool> OnAnswerSelected;
    public event Action<int, int, string> OnAnswerTextFilled;

    #endregion

    public SurveyQuestionUIViewerString(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder) 
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

        // Text only questions dont add answers, we have also have to assign here
        var textField = root.Q<TextField>();

        if (textField != null) {
            textField.RegisterValueChangedCallback(evt => {
                OnAnswerSelected?.Invoke(QuestionID, -1, true);
                OnAnswerTextFilled?.Invoke(QuestionID, -1, evt.newValue);
            });
        }
    }

    public override SurveyAnswerUIBase AddAnswer(string answerText, bool isOther = false) {
        if (_optionsList == null) return null;

        VisualElement answerElement;
        int answerIndex = _addedAnswers.Count;

        if (isOther) {
            // Create a TextField directly for "Other" instead of using the radio template
            var otherField = new TextField();
            answerElement = otherField;
        } else {
            // Use the standard template for normal options
            if (_answerTemplate == null) return null;
            answerElement = _answerTemplate.Instantiate();

            var textLabel = answerElement.Q<Label>();
            if (textLabel != null) textLabel.text = answerText;

            // RadioGroup event has to be assigned here
            var radioGroup = _root.Q<RadioButtonGroup>("options-list");
            radioGroup.RegisterValueChangedCallback(evt =>
            {
                int selectedIndex = evt.newValue;
                OnAnswerSelected?.Invoke(QuestionID, selectedIndex, true);
            });
        }

        // Initialize the logic class
        SurveyAnswerUIViewerString answerUI = new SurveyAnswerUIViewerString(answerElement, answerIndex, this, isOther);

        // Bind Events
        answerUI.OnSelected += (qId, aIdx, val) => OnAnswerSelected?.Invoke(qId, aIdx, val);
        answerUI.OnTextChanged += (qId, aIdx, txt) => OnAnswerTextFilled?.Invoke(qId, aIdx, txt);

        // Layout Handling
        if (isOther) {
            _optionsList.Add(answerElement);
            _otherAnswerUI = answerUI;
        } else {
            InsertAnswerElement(answerElement);
            _addedAnswers.Add(answerUI);
        }

        return answerUI;
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return null;
    }

    protected override void RegisterButtons() {
       
    }

    protected override void RegisterDropdown() {

    }

    protected override void RegisterTextInputs() {

    }

}
