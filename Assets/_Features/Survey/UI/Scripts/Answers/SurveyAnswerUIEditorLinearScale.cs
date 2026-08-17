using SurveySystem;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIEditorLinearScale : SurveyAnswerUIEditor {

    public event Action<int, int, string> OnTextChanged;

    private TextField _textField;
    private Label _minLabel;
    private Label _maxLabel;
    private SliderInt _sliderPreview;
    private Label _draggerTooltip;
    private bool _isDragging = false;

    public SurveyAnswerUIEditorLinearScale(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditorLinearScale questionUI, bool isOther = false)
        : base(answerElement, answerIndex, questionUI, isOther) {

        _textField = _answerElement.Q<TextField>("scale-row-text") ?? _answerElement.Q<TextField>();
        _minLabel = _answerElement.Q<Label>("scale-min-label");
        _maxLabel = _answerElement.Q<Label>("scale-max-label");
        _sliderPreview = _answerElement.Q<SliderInt>("scale-slider-preview") ?? _answerElement.Q<SliderInt>();

        RegisterAnswerEvents();
        SetupDraggerTooltip();
    }

    private void SetupDraggerTooltip() {
        if (_sliderPreview == null) return;

        var dragger = _sliderPreview.Q("unity-dragger") ?? _sliderPreview.Q(className: "unity-base-slider__dragger");
        if (dragger != null && _draggerTooltip == null) {
            _draggerTooltip = new Label(_sliderPreview.value.ToString());
            _draggerTooltip.AddToClassList("scale-dragger-tooltip");
            _draggerTooltip.pickingMode = PickingMode.Ignore;
            dragger.Add(_draggerTooltip);
        }
    }

    protected override void RegisterAnswerEvents() {
        RegisterModalButtonEvents(_answerElement);
        RegisterTextFieldChanges();

        if (_sliderPreview != null) {
            _sliderPreview.RegisterValueChangedCallback(evt => {
                if (_draggerTooltip != null) {
                    _draggerTooltip.text = evt.newValue.ToString();
                }
            });

            _sliderPreview.RegisterCallback<PointerDownEvent>(evt => {
                SetupDraggerTooltip();
                _isDragging = true;
                _sliderPreview.AddToClassList("is-dragging");
                if (_draggerTooltip != null) {
                    _draggerTooltip.AddToClassList("is-dragging");
                    _draggerTooltip.style.opacity = 1f;
                }
            }, TrickleDown.TrickleDown);

            _sliderPreview.RegisterCallback<PointerMoveEvent>(evt => {
                if (_isDragging && _draggerTooltip != null) {
                    _draggerTooltip.style.opacity = 1f;
                }
            }, TrickleDown.TrickleDown);

            _sliderPreview.RegisterCallback<PointerUpEvent>(evt => {
                _isDragging = false;
                _sliderPreview.RemoveFromClassList("is-dragging");
                if (_draggerTooltip != null) {
                    _draggerTooltip.RemoveFromClassList("is-dragging");
                    _draggerTooltip.style.opacity = StyleKeyword.Null;
                }
            }, TrickleDown.TrickleDown);

            _sliderPreview.RegisterCallback<PointerCaptureOutEvent>(evt => {
                _isDragging = false;
                _sliderPreview.RemoveFromClassList("is-dragging");
                if (_draggerTooltip != null) {
                    _draggerTooltip.RemoveFromClassList("is-dragging");
                    _draggerTooltip.style.opacity = StyleKeyword.Null;
                }
            }, TrickleDown.TrickleDown);

            _sliderPreview.RegisterCallback<PointerCancelEvent>(evt => {
                _isDragging = false;
                _sliderPreview.RemoveFromClassList("is-dragging");
                if (_draggerTooltip != null) {
                    _draggerTooltip.RemoveFromClassList("is-dragging");
                    _draggerTooltip.style.opacity = StyleKeyword.Null;
                }
            }, TrickleDown.TrickleDown);
        }

        var deleteBtn = _answerElement.Q<Button>("delete-option-button");
        if (deleteBtn != null) {
            deleteBtn.clicked += () => {
                if (_questionUIRef is SurveyQuestionUIEditorLinearScale linearScaleQ) {
                    linearScaleQ.DeleteAnswer(AnswerIndex);
                }
            };
        }

        var moveUpBtn = _answerElement.Q<Button>("move-up-button");
        if (moveUpBtn != null) {
            moveUpBtn.clicked += () => {
                if (_questionUIRef is SurveyQuestionUIEditorLinearScale linearScaleQ) {
                    linearScaleQ.MoveAnswerUp(AnswerIndex);
                }
            };
        }

        var moveDownBtn = _answerElement.Q<Button>("move-down-button");
        if (moveDownBtn != null) {
            moveDownBtn.clicked += () => {
                if (_questionUIRef is SurveyQuestionUIEditorLinearScale linearScaleQ) {
                    linearScaleQ.MoveAnswerDown(AnswerIndex);
                }
            };
        }
    }

    private void RegisterTextFieldChanges() {
        if (_textField == null) return;

        _textField.RegisterValueChangedCallback(evt => {
            OnTextChanged?.Invoke(_questionUIRef.QuestionID, _answerIndex, evt.newValue);
        });
    }

    public void SetText(string text) {
        if (_textField != null) {
            _textField.value = text;
        }
    }

    public void SetScaleRange(int min, int max) {
        if (_minLabel != null) _minLabel.text = min.ToString();
        if (_maxLabel != null) _maxLabel.text = max.ToString();
        if (_sliderPreview != null) {
            _sliderPreview.lowValue = min;
            _sliderPreview.highValue = max;
            int val = (min + max) / 2;
            _sliderPreview.value = val;

            if (_draggerTooltip != null) {
                _draggerTooltip.text = val.ToString();
            }
        }
    }
}
