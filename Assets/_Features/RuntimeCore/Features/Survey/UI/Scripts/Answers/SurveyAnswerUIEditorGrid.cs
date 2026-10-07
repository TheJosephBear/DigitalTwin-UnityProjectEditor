using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIEditorGrid : SurveyAnswerUIEditor {

    private RadioButtonGroup _radioGroup;

    public string Text { get; private set; } = string.Empty;

    public event Action<int, int, string> OnTextChanged;

    public SurveyAnswerUIEditorGrid(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditorGrid questionUI, bool isOther)
        : base(answerElement, answerIndex, questionUI, isOther) {
    }

    protected override void RegisterAnswerEvents() {
        RegisterTextField();
    }

    private void RegisterTextField() {
        var textField = _answerElement.Q<TextField>("column-title") ?? _answerElement.Q<TextField>("row-title") ?? _answerElement.Q<TextField>();
        if (textField == null) return;

        textField.RegisterValueChangedCallback(evt =>
        {
            Text = evt.newValue;
            OnTextChanged?.Invoke(_questionUIRef.QuestionID, _answerIndex, evt.newValue);
        });
    }

    public void InvokeTextChanged(string newText) {
        InvokeTextChanged(_answerIndex, newText);
    }

    public void InvokeTextChanged(int actualIndex, string newText) {
        Text = newText;
        UpdateIndex(actualIndex);
        OnTextChanged?.Invoke(_questionUIRef.QuestionID, actualIndex, newText);
    }

    public void RebuildRadioButtons(int columnCount, bool isCheckbox) {
        _radioGroup = _answerElement.Q<RadioButtonGroup>();

        if (_radioGroup == null)
            return;

        var content = _radioGroup.contentContainer;
        content.Clear();

        for (int i = 0; i < columnCount; i++) {
            if (isCheckbox) {
                var button = new Toggle();
                content.Add(button);
            } else {
                var button = new CustomRadioButtonNoText();
                content.Add(button);
            }
        }
    }

    public void SetText(string text) {
        Text = text;
        var textField = _answerElement.Q<TextField>("column-title") ?? _answerElement.Q<TextField>("row-title") ?? _answerElement.Q<TextField>();
        if (textField == null) return;
        textField.value = text;
    }
}
