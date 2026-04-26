using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIViewerString : SurveyAnswerUIViewer {
    // We pass the QuestionID down so the Answer knows who it belongs to when firing events
    public event Action<int, int, bool> OnSelected;
    public event Action<int, int, string> OnTextChanged;

    public SurveyAnswerUIViewerString(VisualElement answerElement, int answerIndex, SurveyQuestionUIViewer questionUI, bool isOther)
        : base(answerElement, answerIndex, questionUI, isOther) {

        RegisterAnswerEvents();
    }

    protected override void RegisterAnswerEvents() {
        if (AnswerElement == null) return;

        // 1. Logic for "Other" (TextField is either the root or a child)
        var textField = AnswerElement as TextField ?? AnswerElement.Q<TextField>();

        if (_isOther && textField != null) {
            textField.RegisterValueChangedCallback(evt => {
                // If user types, we mark this answer as selected
                bool hasText = !string.IsNullOrEmpty(evt.newValue);
                OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, hasText);
                OnTextChanged?.Invoke(_questionUIRef.QuestionID, AnswerIndex, evt.newValue);
            });
            return; // Skip radio logic for "Other"
        }

        // 2. Logic for Normal Answers (Radio/Toggle selection)
        var customRadio = AnswerElement.Q<CustomRadioButton>();
        if (customRadio != null && customRadio.Radio != null) {
            customRadio.Radio.RegisterValueChangedCallback(evt => {
                if (evt.newValue) {
                    OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
                }
            });
        } else {
            var toggle = AnswerElement.Q<Toggle>();
            if (toggle != null) {
                toggle.RegisterValueChangedCallback(evt => {
                    OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, evt.newValue);
                });
            }
        }
    }
}