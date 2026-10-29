using SurveySystem;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIEditorLinearScale : SurveyAnswerUIEditor {

    public event Action<int, int, string> OnTextChanged;
    public event Action<int, int, string> OnRightTextChanged;

    private TextField _textField;
    private TextField _rightTextField;
    private Label _minLabel;
    private Label _maxLabel;
    private SliderInt _sliderPreview;

    public SurveyAnswerUIEditorLinearScale(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditorLinearScale questionUI, bool isOther = false)
        : base(answerElement, answerIndex, questionUI, isOther) {

        _textField = _answerElement.Q<TextField>("scale-row-text") ?? _answerElement.Q<TextField>();
        _rightTextField = _answerElement.Q<TextField>("scale-row-text-right");
        _minLabel = _answerElement.Q<Label>("scale-min-label");
        _maxLabel = _answerElement.Q<Label>("scale-max-label");
        _sliderPreview = _answerElement.Q<SliderInt>("scale-slider-preview") ?? _answerElement.Q<SliderInt>();

        RegisterAnswerEvents();
        DisableSliderInteraction();
    }

    // Only a preview of the scale: the admin doesn't answer it, so it ignores the pointer (and keeps its look)
    private void DisableSliderInteraction() {
        if (_sliderPreview == null) return;
        _sliderPreview.focusable = false;
        _sliderPreview.Query<VisualElement>().ForEach(element => element.pickingMode = PickingMode.Ignore);

        // Preview has no value, its handle always sits in the middle (also on 0–5 and 1–10)
        _sliderPreview.RegisterCallback<GeometryChangedEvent>(_ => SurveyUIUtils.CenterSliderHandle(_sliderPreview, true));
    }

    protected override void RegisterAnswerEvents() {
        RegisterModalButtonEvents(_answerElement);
        RegisterTextFieldChanges();
    }

    private void RegisterTextFieldChanges() {
        // Fields are assigned after the base constructor's first call, so this registers only once
        _textField?.RegisterValueChangedCallback(evt => {
            OnTextChanged?.Invoke(_questionUIRef.QuestionID, _answerIndex, evt.newValue);
        });

        _rightTextField?.RegisterValueChangedCallback(evt => {
            OnRightTextChanged?.Invoke(_questionUIRef.QuestionID, _answerIndex, evt.newValue);
        });
    }

    public void SetText(string text) {
        if (_textField != null) {
            _textField.value = text;
        }
    }

    public void SetRightText(string text) {
        if (_rightTextField != null) {
            _rightTextField.value = text ?? string.Empty;
        }
    }

    public void SetScaleRange(int min, int max) {
        if (_minLabel != null) _minLabel.text = min.ToString();
        if (_maxLabel != null) _maxLabel.text = max.ToString();
        if (_sliderPreview != null) {
            _sliderPreview.lowValue = min;
            _sliderPreview.highValue = max;
            _sliderPreview.value = (min + max) / 2;
            SurveyUIUtils.CenterSliderHandle(_sliderPreview, true);
        }
    }
}
