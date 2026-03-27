using SurveySystem;
using System;
using System.Collections.Generic;
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

    #region Events

    public event Action<int, int> OnAnswerSelected;
    public event Action<int, int, string> OnAnswerTextFilled;

    #endregion

    public SurveyQuestionUIViewer(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints) {
        _root = root;
        QuestionID = questionId;
        _questionType = questionType;
        _viewPoints = viewPoints;

        QuestionUIMapping mapping = UnityEngine.Object.FindFirstObjectByType<QuestionUIMapping>();
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
        var field = _root.Q<Label>("question-title");
        if (field != null) field.text = title;
    }

    public void SetDescription(string desc) {
        var field = _root.Q<Label>("question-description");
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

            var textLabel = answerElement.Q<Label>();
            if (textLabel != null) textLabel.text = answerText;

            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, null);
            _addedAnswers.Add(answerUI);

            RegisterAnswerCallbacks(answerUI, answerIndex);
        }
    }

    private void RegisterAnswerCallbacks(SurveyAnswerUI answerUI, int index) {
        var root = answerUI.AnswerElement;

        var customRadio = root.Q<CustomRadioButton>();
        if (customRadio != null) {
            customRadio.Radio.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue) {
                    OnAnswerSelected?.Invoke(QuestionID, index);
                }
            });
        }

        var textField = root.Q<TextField>();
        if (textField != null) {
            textField.RegisterValueChangedCallback(evt =>
            {
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