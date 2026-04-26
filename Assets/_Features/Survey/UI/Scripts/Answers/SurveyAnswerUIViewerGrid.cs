using UnityEngine.UIElements;
using System;

public class SurveyAnswerUIViewerGrid : SurveyAnswerUIBase {
    private VisualElement _radioContainer;
    private SurveyQuestionUIViewerGrid _gridRef;

    public SurveyAnswerUIViewerGrid(VisualElement element, int index, SurveyQuestionUIViewerGrid questionUI)
        : base(element, index, questionUI, false) {
        _gridRef = questionUI;
        _radioContainer = element.Q<VisualElement>("radio-container") ?? element.Q<RadioButtonGroup>();
    }

    protected override void RegisterAnswerEvents() { } // Static in viewer

    public void SetRowText(string text) {
        var label = _answerElement.Q<Label>();
        if (label != null) label.text = text;
    }

    public void RebuildRadioButtons(int columnCount, bool isCheckbox) {
        if (_radioContainer == null) return;
        _radioContainer.Clear();

        for (int i = 0; i < columnCount; i++) {
            int colIndex = i;
            if (isCheckbox) {
                var toggle = new Toggle();
                toggle.RegisterValueChangedCallback(evt => {
                    _gridRef.InvokeAnswerSelected(_answerIndex, colIndex, evt.newValue);
                });
                _radioContainer.Add(toggle);
            } else {
                // Using a standard RadioButton. For proper grouping, 
                // ensure they are children of the same parent or a RadioButtonGroup.
                var radio = new CustomRadioButtonNoText();
                radio.RegisterRadioCallback(evt => {
                    if (evt.newValue) _gridRef.InvokeAnswerSelected(_answerIndex, colIndex, true);
                });
                _radioContainer.Add(radio);
            }
        }
    }
}