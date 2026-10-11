using System;
using UnityEngine;
using UnityEngine.UIElements;
using UIRadioButton = UnityEngine.UIElements.RadioButton;

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

        var textField = AnswerElement.Q<TextField>();
        var radio = AnswerElement.Q<UIRadioButton>();
        var toggle = AnswerElement.Q<Toggle>();
        var customRadio = AnswerElement.Q<CustomRadioButton>();

        // Register click callback on the first VisualElement (option-row) and AnswerElement
        var optionRow = AnswerElement.Q<VisualElement>(className: "option-row") ?? (AnswerElement.childCount > 0 ? AnswerElement[0] : AnswerElement);

        void HandleRowClick(ClickEvent evt) {
            if (evt.target is TextField || (evt.target as VisualElement)?.GetFirstAncestorOfType<TextField>() != null) {
                return;
            }

            if (radio != null) {
                if (!radio.value) {
                    radio.value = true;
                }
                if (_questionUIRef is SurveyQuestionUIViewerString viewerString) {
                    viewerString.SelectAnswerRadio(AnswerIndex);
                }
                OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
            } else if (toggle != null) {
                if (evt.target != toggle && (evt.target as VisualElement)?.GetFirstAncestorOfType<Toggle>() != toggle) {
                    toggle.value = !toggle.value;
                }
            } else if (customRadio != null && customRadio.Radio != null) {
                if (!customRadio.Radio.value) {
                    customRadio.Radio.value = true;
                }
                if (_questionUIRef is SurveyQuestionUIViewerString viewerString) {
                    viewerString.SelectAnswerRadio(AnswerIndex);
                }
                OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
            }
        }

        optionRow.RegisterCallback<ClickEvent>(HandleRowClick);
        if (AnswerElement != optionRow) {
            AnswerElement.RegisterCallback<ClickEvent>(HandleRowClick);
        }

        if (_isOther) {
            if (textField != null) {
                textField.RegisterValueChangedCallback(evt => {
                    bool hasText = !string.IsNullOrEmpty(evt.newValue);
                    if (radio != null && hasText) {
                        radio.value = true;
                        if (_questionUIRef is SurveyQuestionUIViewerString viewerString) {
                            viewerString.SelectAnswerRadio(AnswerIndex);
                        }
                    }
                    if (toggle != null && hasText) toggle.value = true;
                    OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
                    OnTextChanged?.Invoke(_questionUIRef.QuestionID, AnswerIndex, evt.newValue);
                });
            }

            if (radio != null) {
                radio.RegisterValueChangedCallback(evt => {
                    if (evt.newValue) {
                        if (_questionUIRef is SurveyQuestionUIViewerString viewerString) {
                            viewerString.SelectAnswerRadio(AnswerIndex);
                        }
                        OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
                    }
                });
            } else if (toggle != null) {
                toggle.RegisterValueChangedCallback(evt => {
                    OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, evt.newValue);
                });
            }
            return;
        }

        // Logic for Normal Answers (Radio/Toggle selection)
        if (customRadio != null && customRadio.Radio != null) {
            customRadio.Radio.RegisterValueChangedCallback(evt => {
                if (evt.newValue) {
                    if (_questionUIRef is SurveyQuestionUIViewerString viewerString) {
                        viewerString.SelectAnswerRadio(AnswerIndex);
                    }
                    OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
                }
            });
        } else if (radio != null) {
            radio.RegisterValueChangedCallback(evt => {
                if (evt.newValue) {
                    if (_questionUIRef is SurveyQuestionUIViewerString viewerString) {
                        viewerString.SelectAnswerRadio(AnswerIndex);
                    }
                    OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
                }
            });
        } else if (toggle != null) {
            toggle.RegisterValueChangedCallback(evt => {
                OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, evt.newValue);
            });
        }
    }
}