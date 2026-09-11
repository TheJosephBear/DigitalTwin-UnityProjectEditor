using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UIRadioButton = UnityEngine.UIElements.RadioButton;

public class SurveyQuestionUIViewerString : SurveyQuestionUIViewer {

    #region Events
    public event Action<int, int, bool> OnAnswerSelected;
    public event Action<int, int, string> OnAnswerTextFilled;

    #endregion

    private bool _isRadioGroupRegistered = false;

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
            if (_answerTemplate != null) {
                answerElement = _answerTemplate.Instantiate();
            } else {
                answerElement = new VisualElement();
                answerElement.AddToClassList("option-row");
                var toggle = new Toggle();
                answerElement.Add(toggle);
            }

            var optionRow = answerElement.Q<VisualElement>(className: "option-row") ?? answerElement;
            var radio = answerElement.Q<UIRadioButton>();
            var toggleEl = answerElement.Q<Toggle>();
            var textLabel = answerElement.Q<Label>();

            if (radio != null) {
                radio.text = string.Empty;
                radio.label = string.Empty;
                radio.style.flexGrow = 0;
                radio.style.flexDirection = FlexDirection.Row;
                radio.style.marginRight = 8;
                radio.style.marginLeft = 0;
            }
            if (toggleEl != null) {
                toggleEl.text = string.Empty;
                toggleEl.label = string.Empty;
                toggleEl.style.flexGrow = 0;
                toggleEl.style.flexDirection = FlexDirection.Row;
                toggleEl.style.marginRight = 8;
                toggleEl.style.marginLeft = 0;
            }
            if (textLabel != null && (object)textLabel != radio && (object)textLabel != toggleEl) {
                textLabel.text = string.Empty;
            }

            var otherField = new TextField();
            otherField.name = "other-text-field";
            otherField.textEdition.placeholder = string.IsNullOrWhiteSpace(answerText) ? "Jiná..." : answerText;
            otherField.style.flexGrow = 1;
            otherField.style.whiteSpace = WhiteSpace.Normal;
            otherField.style.marginLeft = 8;
            otherField.AddToClassList("other-text-input");

            void StyleTextInput(VisualElement inputEl) {
                if (inputEl == null) return;
                inputEl.style.backgroundColor = Color.white;
                inputEl.style.borderTopWidth = 1;
                inputEl.style.borderBottomWidth = 1;
                inputEl.style.borderLeftWidth = 1;
                inputEl.style.borderRightWidth = 1;
                Color borderCol = new Color(0, 0, 0, 0.25f);
                inputEl.style.borderTopColor = borderCol;
                inputEl.style.borderBottomColor = borderCol;
                inputEl.style.borderLeftColor = borderCol;
                inputEl.style.borderRightColor = borderCol;
                inputEl.style.borderTopLeftRadius = 6;
                inputEl.style.borderTopRightRadius = 6;
                inputEl.style.borderBottomLeftRadius = 6;
                inputEl.style.borderBottomRightRadius = 6;
                inputEl.style.paddingTop = 6;
                inputEl.style.paddingBottom = 6;
                inputEl.style.paddingLeft = 10;
                inputEl.style.paddingRight = 10;
            }

            otherField.RegisterCallback<AttachToPanelEvent>((evt) => {
                var innerInput = otherField.Q(className: "unity-base-text-field__input") ?? otherField.Q("unity-text-input");
                StyleTextInput(innerInput);
            });
            var immediateInput = otherField.Q(className: "unity-base-text-field__input") ?? otherField.Q("unity-text-input");
            StyleTextInput(immediateInput);

            optionRow.AddToClassList("option-row--other");
            optionRow.Add(otherField);
        } else {
            // Use the standard template for normal options
            if (_answerTemplate == null) return null;
            answerElement = _answerTemplate.Instantiate();

            var radio = answerElement.Q<UIRadioButton>();
            if (radio != null) {
                radio.label = string.Empty;
                radio.text = answerText;
            }
            var toggle = answerElement.Q<Toggle>();
            if (toggle != null) {
                toggle.label = string.Empty;
                toggle.text = answerText;
            }
            var textLabel = answerElement.Q<Label>();
            if (textLabel != null && (object)textLabel != radio && (object)textLabel != toggle && 
                !textLabel.ClassListContains("unity-radio-button__text") && !textLabel.ClassListContains("unity-toggle__text") &&
                !textLabel.ClassListContains("unity-base-field__label")) {
                textLabel.text = answerText;
            }
        }

        // RadioGroup event
        var radioGroup = _root.Q<RadioButtonGroup>("options-list");
        if (radioGroup != null && !_isRadioGroupRegistered) {
            _isRadioGroupRegistered = true;
            radioGroup.RegisterValueChangedCallback(evt => {
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

    public void SelectAnswerRadio(int selectedIndex) {
        if (_questionType != QuestionType.MultipleChoiceSingle) return;

        for (int i = 0; i < _addedAnswers.Count; i++) {
            if (_addedAnswers[i]?.AnswerElement != null) {
                var radio = _addedAnswers[i].AnswerElement.Q<UIRadioButton>();
                if (radio != null) {
                    radio.SetValueWithoutNotify(i == selectedIndex);
                }
                var customRadio = _addedAnswers[i].AnswerElement.Q<CustomRadioButton>();
                if (customRadio != null && customRadio.Radio != null) {
                    customRadio.Radio.SetValueWithoutNotify(i == selectedIndex);
                }
            }
        }

        if (_otherAnswerUI?.AnswerElement != null) {
            var radio = _otherAnswerUI.AnswerElement.Q<UIRadioButton>();
            if (radio != null) {
                radio.SetValueWithoutNotify(_otherAnswerUI.AnswerIndex == selectedIndex);
            }
            var customRadio = _otherAnswerUI.AnswerElement.Q<CustomRadioButton>();
            if (customRadio != null && customRadio.Radio != null) {
                customRadio.Radio.SetValueWithoutNotify(_otherAnswerUI.AnswerIndex == selectedIndex);
            }
        }

        var radioGroup = _root.Q<RadioButtonGroup>("options-list");
        if (radioGroup != null) {
            radioGroup.SetValueWithoutNotify(selectedIndex);
        }
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return null;
    }

    protected override void RegisterButtons() {
        base.RegisterButtons();
    }

    protected override void RegisterDropdown() {

    }

    protected override void RegisterTextInputs() {

    }

}
