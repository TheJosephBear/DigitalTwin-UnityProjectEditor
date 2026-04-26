using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIViewerString : SurveyQuestionUIViewer {

    #region Events

    public event Action<int, int> OnAnswerSelected;
    public event Action<int, int, string> OnAnswerTextFilled;

    #endregion

    public SurveyQuestionUIViewerString(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder) 
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

    }

    public override void AddAnswer(string answerText, bool isOther = false) {
        if (_optionsList == null || _answerTemplate == null) return;

        TemplateContainer answerElement = _answerTemplate.Instantiate();

        int answerIndex = _addedAnswers.Count;

        if (isOther) {
            _optionsList.Add(answerElement);

            SurveyAnswerUIEditorString answerUI = new SurveyAnswerUIEditorString(answerElement, answerIndex, null, isOther);
            _otherAnswerUI = answerUI;
        } else {
            if (_otherAnswerUI != null) {
                int insertIndex = _optionsList.IndexOf(_otherAnswerUI.AnswerElement);
                _optionsList.Insert(insertIndex, answerElement);
            } else {
                _optionsList.Add(answerElement);
            }

            var textLabel = answerElement.Q<UnityEngine.UIElements.Label>();
            if (textLabel != null) textLabel.text = answerText;

            SurveyAnswerUIEditorString answerUI = new SurveyAnswerUIEditorString(answerElement, answerIndex, null, isOther);
            _addedAnswers.Add(answerUI);

            RegisterAnswerCallbacks(answerUI, answerIndex);
        }
    }

    private void RegisterAnswerCallbacks(SurveyAnswerUIEditorString answerUI, int index) {
        var root = answerUI.AnswerElement;
        if (root == null) {
            //      Debug.LogError($"[Survey] AnswerElement for index {index} is null!");
            return;
        }

        // 1. Check for the class directly
        var customRadio = root.Q<CustomRadioButton>();

        // 2. If null, search deeper using a Query
        if (customRadio == null) {
            //     Debug.LogWarning($"[Survey] Q<CustomRadioButton> failed for index {index}. Searching via Query...");
            customRadio = root.Query<CustomRadioButton>().First();
        }

        if (customRadio != null) {
            //     Debug.Log($"[Survey] Successfully found CustomRadioButton for index {index}. Registering callback.");

            customRadio.RegisterRadioCallback(evt => {
                //       Debug.Log($"[Survey] Internal Radio Toggle detected for index {index}. New Value: {evt.newValue}");
                if (evt.newValue) {
                    //           Debug.Log($"[Survey] Invoking OnAnswerSelected for Question {QuestionID}, Index {index}");
                    OnAnswerSelected?.Invoke(QuestionID, index);
                }
            });
        } else {
            // 3. Last resort: Check if the element exists but just isn't being cast correctly
            var anyElementNamedRadio = root.Q("my-radio-name"); // Replace with the name used in UXML if applicable
                                                                //   Debug.LogError($"[Survey] CRITICAL: Could not find CustomRadioButton for index {index}. " +
                                                                //                   $"Total children in root: {root.childCount}. " +
                                                                //                   $"Is root a TemplateContainer? {root is TemplateContainer}");

            // Let's try to find the raw RadioButton inside the custom element
            var rawRadio = root.Q<UnityEngine.UIElements.RadioButton>();
            if (rawRadio != null) {
                //     Debug.LogWarning($"[Survey] Found a raw RadioButton for index {index} even though CustomRadioButton lookup failed. Hooking directly to raw radio.");
                rawRadio.RegisterValueChangedCallback(evt => {
                    if (evt.newValue) OnAnswerSelected?.Invoke(QuestionID, index);
                });
            }
        }

        // TextField Debugging
        var textField = root.Q<TextField>();
        if (textField != null) {
            //    Debug.Log($"[Survey] TextField found for index {index}.");
            textField.RegisterValueChangedCallback(evt => {
                //        Debug.Log($"[Survey] Text changed for index {index}: {evt.newValue}");
                OnAnswerTextFilled?.Invoke(QuestionID, index, evt.newValue);
            });
        }
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
