using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIViewerLinearScale : SurveyAnswerUIViewer {

    public event Action<int, int, int> OnValueChanged;

    private Label _rowLabel;
    private Label _minLabel;
    private Label _maxLabel;
    private Label _valueBadge;
    private SliderInt _slider;
    private Label _draggerTooltip;
    private bool _isDragging = false;
    private bool _hasValue = false; // Row counts as answered only after user interaction

    private const string UnsetClass = "scale-viewer-row--unset";
    private const string NoValueText = "–";

    public int CurrentValue => _slider != null ? _slider.value : 0;
    public bool HasValue => _hasValue;

    public SurveyAnswerUIViewerLinearScale(VisualElement answerElement, int answerIndex, SurveyQuestionUIViewer questionUI, bool isOther = false)
        : base(answerElement, answerIndex, questionUI, isOther) {

        _rowLabel = _answerElement.Q<Label>("scale-row-label") ?? _answerElement.Q<Label>();
        _minLabel = _answerElement.Q<Label>("scale-min-label");
        _maxLabel = _answerElement.Q<Label>("scale-max-label");
        _valueBadge = _answerElement.Q<Label>("scale-value-badge");
        _slider = _answerElement.Q<SliderInt>("scale-slider") ?? _answerElement.Q<SliderInt>();

        RegisterAnswerEvents();
        SetupDraggerTooltip();
        _answerElement?.AddToClassList(UnsetClass);
    }

    private void SetupDraggerTooltip() {
        if (_slider == null) return;

        var dragger = _slider.Q("unity-dragger") ?? _slider.Q(className: "unity-base-slider__dragger");
        if (dragger != null && _draggerTooltip == null) {
            _draggerTooltip = new Label(_hasValue ? _slider.value.ToString() : NoValueText);
            _draggerTooltip.AddToClassList("scale-dragger-tooltip");
            _draggerTooltip.pickingMode = PickingMode.Ignore;
            dragger.Add(_draggerTooltip);
        }
    }

    protected override void RegisterAnswerEvents() {
        if (_slider == null) return;

        _slider.RegisterValueChangedCallback(evt => {
            MarkAsSet();
            UpdateValueLabels(evt.newValue.ToString());
            OnValueChanged?.Invoke(_questionUIRef.QuestionID, AnswerIndex, evt.newValue);
        });

        _slider.RegisterCallback<PointerDownEvent>(evt => {
            SetupDraggerTooltip();
            _isDragging = true;
            _slider.AddToClassList("is-dragging");
            if (_draggerTooltip != null) {
                _draggerTooltip.AddToClassList("is-dragging");
                _draggerTooltip.style.opacity = 1f;
            }
        }, TrickleDown.TrickleDown);

        _slider.RegisterCallback<PointerMoveEvent>(evt => {
            if (_isDragging && _draggerTooltip != null) {
                _draggerTooltip.style.opacity = 1f;
            }
        }, TrickleDown.TrickleDown);

        _slider.RegisterCallback<PointerUpEvent>(evt => {
            ConfirmUntouchedValue();
            _isDragging = false;
            _slider.RemoveFromClassList("is-dragging");
            if (_draggerTooltip != null) {
                _draggerTooltip.RemoveFromClassList("is-dragging");
                _draggerTooltip.style.opacity = StyleKeyword.Null;
            }
        }, TrickleDown.TrickleDown);

        _slider.RegisterCallback<PointerCaptureOutEvent>(evt => {
            ConfirmUntouchedValue();
            _isDragging = false;
            _slider.RemoveFromClassList("is-dragging");
            if (_draggerTooltip != null) {
                _draggerTooltip.RemoveFromClassList("is-dragging");
                _draggerTooltip.style.opacity = StyleKeyword.Null;
            }
        }, TrickleDown.TrickleDown);

        _slider.RegisterCallback<PointerCancelEvent>(evt => {
            _isDragging = false;
            _slider.RemoveFromClassList("is-dragging");
            if (_draggerTooltip != null) {
                _draggerTooltip.RemoveFromClassList("is-dragging");
                _draggerTooltip.style.opacity = StyleKeyword.Null;
            }
        }, TrickleDown.TrickleDown);
    }

    // Clicking the slider on its current value doesn't raise ValueChanged, so register it here
    private void ConfirmUntouchedValue() {
        if (_hasValue || _slider == null) return;
        MarkAsSet();
        UpdateValueLabels(_slider.value.ToString());
        OnValueChanged?.Invoke(_questionUIRef.QuestionID, AnswerIndex, _slider.value);
    }

    private void MarkAsSet() {
        if (_hasValue) return;
        _hasValue = true;
        _answerElement?.RemoveFromClassList(UnsetClass);
    }

    private void UpdateValueLabels(string text) {
        if (_valueBadge != null) {
            _valueBadge.text = text;
        }
        if (_draggerTooltip != null) {
            _draggerTooltip.text = text;
        }
    }

    public void SetText(string text) {
        if (_rowLabel != null) {
            _rowLabel.text = text;
        }
    }

    public void SetScaleRange(int min, int max, int? initialValue = null) {
        if (_minLabel != null) _minLabel.text = min.ToString();
        if (_maxLabel != null) _maxLabel.text = max.ToString();
        if (_slider != null) {
            _slider.lowValue = min;
            _slider.highValue = max;
            int val = initialValue ?? ((min + max) / 2);
            _slider.SetValueWithoutNotify(val);
            if (initialValue.HasValue) MarkAsSet();

            UpdateValueLabels(_hasValue ? val.ToString() : NoValueText);
        }
    }
}
