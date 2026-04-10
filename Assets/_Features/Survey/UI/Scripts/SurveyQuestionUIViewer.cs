using SurveySystem;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIViewer : ISurveyQuestionViewerUI {
    public int QuestionID { get; }
    private VisualElement _root;
    private QuestionType _questionType;
    private List<SerializableViewPoint> _viewPoints;

    private List<SurveyAnswerUI> _addedAnswers = new List<SurveyAnswerUI>();
    private SurveyAnswerUI _otherAnswerUI;
    private VisualElement _optionsList;

    private VisualTreeAsset _answerTemplate;

    public VisualElement QuestionElement => _root;
    private SurveyUIBuilder _surveyUIBuilder;

    #region Events

    public event Action<int, int> OnAnswerSelected;
    public event Action<int, int, string> OnAnswerTextFilled;

    #endregion

    public SurveyQuestionUIViewer(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder) {
        _root = root;
        _surveyUIBuilder = uiBuilder;
        QuestionID = questionId;
        _questionType = questionType;
        _viewPoints = viewPoints;

        QuestionUIMapping mapping = _surveyUIBuilder.questionUIMapping;
        if (mapping != null) {
            _answerTemplate = mapping.GetAnswerUITemplate(_questionType);
        }

        if (_root == null) return;

        _optionsList = _root.Q<RadioButtonGroup>("options-list")
                    ?? _root.Q<VisualElement>("options-list");

        _optionsList?.Clear();

        RegisterInputs();
    }

    private void RegisterInputs() {
        var cameraViewDropdown = _root.Q<DropdownField>("camera-view-dropdown");

        if (cameraViewDropdown != null) {
            cameraViewDropdown.RegisterValueChangedCallback(evt => {
                int index = cameraViewDropdown.index;
                if (index >= 0 && index < _viewPoints.Count) {
                    // optional: hook if needed later
                }
            });

            PopulateCameraViewDropdown(cameraViewDropdown);
        }
    }

    public void SetTitle(string title) {
        var field = _root.Q<UnityEngine.UIElements.Label>("question-title");
        if (field != null) field.text = title;
    }

    public void SetDescription(string desc) {
        var field = _root.Q<UnityEngine.UIElements.Label>("question-description");
        if (field != null) field.text = desc;
    }

    public void AddAnswer(string answerText, bool isOther = false) {
        if (_optionsList == null || _answerTemplate == null) return;

        TemplateContainer answerElement = _answerTemplate.Instantiate();

        int answerIndex = _addedAnswers.Count;

        if (isOther) {
            _optionsList.Add(answerElement);

            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, null);
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

            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, null);
            _addedAnswers.Add(answerUI);

            RegisterAnswerCallbacks(answerUI, answerIndex);
        }
    }

    private void RegisterAnswerCallbacks(SurveyAnswerUI answerUI, int index) {
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

    private void PopulateCameraViewDropdown(DropdownField dropdown) {
        if (dropdown == null) return;

        List<string> choiceLabels = new List<string>();

        foreach (var viewPoint in _viewPoints) {
            choiceLabels.Add(viewPoint.Name);
        }

        dropdown.choices = choiceLabels;

        if (dropdown.choices.Count > 0) {
            dropdown.value = dropdown.choices[0];
        }
    }
}