using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIViewerLinearScale : SurveyQuestionUIViewer {

    public event Action<int, int, int> OnScaleValueChanged;

    private int _min = 1;
    private int _max = 5;
    private string _scaleType = "1 - 5";

    public SurveyQuestionUIViewerLinearScale(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

        // In viewer, hide scale configuration bar if present in template
        var configBar = _root.Q<VisualElement>("scale-config-bar");
        if (configBar != null) {
            configBar.style.display = DisplayStyle.None;
        }

        var addBtn = _root.Q<Button>("add-option-button");
        if (addBtn != null) {
            addBtn.style.display = DisplayStyle.None;
        }
    }

    public void SetScaleRange(string scaleType, int min, int max) {
        _scaleType = !string.IsNullOrEmpty(scaleType) ? scaleType : $"{min} - {max}";
        _min = min;
        _max = max;

        foreach (var answer in _addedAnswers) {
            if (answer is SurveyAnswerUIViewerLinearScale scaleAnswer) {
                scaleAnswer.SetScaleRange(_min, _max);
            }
        }
    }

    public override SurveyAnswerUIBase AddAnswer(string answerText, bool isOther = false) {
        if (_optionsList == null) return null;

        VisualElement rowElement = CreateViewerRowElement();
        int answerIndex = _addedAnswers.Count;

        SurveyAnswerUIViewerLinearScale answerUI = new SurveyAnswerUIViewerLinearScale(rowElement, answerIndex, this, isOther);
        answerUI.SetText(answerText);
        answerUI.SetScaleRange(_min, _max);

        answerUI.OnValueChanged += (qId, aIdx, val) => {
            OnScaleValueChanged?.Invoke(qId, aIdx, val);
        };

        _optionsList.Add(rowElement);
        _addedAnswers.Add(answerUI);

        return answerUI;
    }

    private VisualElement CreateViewerRowElement() {
        var row = new VisualElement();
        row.AddToClassList("scale-viewer-row");

        var label = new Label();
        label.name = "scale-row-label";
        label.AddToClassList("scale-viewer-row-label");
        row.Add(label);

        var sliderContainer = new VisualElement();
        sliderContainer.AddToClassList("scale-preview-container");

        var minLabel = new Label(_min.ToString());
        minLabel.name = "scale-min-label";
        minLabel.AddToClassList("scale-limit-label");
        sliderContainer.Add(minLabel);

        int defaultVal = (_min + _max) / 2;
        var slider = new SliderInt();
        slider.name = "scale-slider";
        slider.lowValue = _min;
        slider.highValue = _max;
        slider.value = defaultVal;
        slider.AddToClassList("scale-slider");
        sliderContainer.Add(slider);

        var maxLabel = new Label(_max.ToString());
        maxLabel.name = "scale-max-label";
        maxLabel.AddToClassList("scale-limit-label");
        sliderContainer.Add(maxLabel);

        var valueBadge = new Label(defaultVal.ToString());
        valueBadge.name = "scale-value-badge";
        valueBadge.AddToClassList("scale-value-badge");
        sliderContainer.Add(valueBadge);

        row.Add(sliderContainer);
        return row;
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return null;
    }

    protected override void RegisterDropdown() {
    }

    protected override void RegisterTextInputs() {
    }
}
