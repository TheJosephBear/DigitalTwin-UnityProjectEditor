using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIEditorLinearScale : SurveyQuestionUIEditor {

    #region Scale Presets
    public static readonly List<string> ScaleTypeChoices = new() {
        "1 - 5",
        "-3 - 3",
        "1 - 7",
        "0 - 10",
        "1 - 10",
        "-5 - 5",
        "0 - 5"
    };

    public static (int min, int max) ParseScaleRange(string scaleType) {
        if (string.IsNullOrEmpty(scaleType)) return (1, 5);

        string[] parts = scaleType.Split(new[] { " - ", " až ", " to " }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int min) && int.TryParse(parts[1].Trim(), out int max)) {
            return (min, max);
        }

        return scaleType switch {
            "-3 - 3" => (-3, 3),
            "1 - 5" => (1, 5),
            "1 - 7" => (1, 7),
            "0 - 10" => (0, 10),
            "1 - 10" => (1, 10),
            "-5 - 5" => (-5, 5),
            "0 - 5" => (0, 5),
            _ => (1, 5)
        };
    }
    #endregion

    #region Events
    public event Action<int, SurveyAnswerUIBase> OnAnswerAdded;
    public event Action<int, int> OnAnswerRemoved;
    public event Action<int, string, int, int> OnScaleTypeChanged;
    #endregion

    private int _min = 1;
    private int _max = 5;
    private string _scaleType = "1 - 5";
    private DropdownField _scaleTypeDropdown;

    public int Min => _min;
    public int Max => _max;
    public string ScaleType => _scaleType;

    public SurveyQuestionUIEditorLinearScale(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder,
        bool isDeserialized = false)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

        InitializeScaleTypeDropdown();
    }

    private void InitializeScaleTypeDropdown() {
        _scaleTypeDropdown = _root.Q<DropdownField>("scale-type-dropdown");
        if (_scaleTypeDropdown != null) {
            _scaleTypeDropdown.choices = ScaleTypeChoices;
            _scaleTypeDropdown.value = _scaleType;

            _scaleTypeDropdown.RegisterValueChangedCallback(evt => {
                var (newMin, newMax) = ParseScaleRange(evt.newValue);
                _scaleType = evt.newValue;
                _min = newMin;
                _max = newMax;

                UpdateAllRowsScaleRange();
                OnScaleTypeChanged?.Invoke(QuestionID, _scaleType, _min, _max);
            });
        }
    }

    public void SetScaleRange(string scaleType, int min, int max) {
        _scaleType = !string.IsNullOrEmpty(scaleType) ? scaleType : $"{min} - {max}";
        _min = min;
        _max = max;

        if (_scaleTypeDropdown != null) {
            if (!_scaleTypeDropdown.choices.Contains(_scaleType)) {
                _scaleTypeDropdown.choices.Add(_scaleType);
            }
            _scaleTypeDropdown.SetValueWithoutNotify(_scaleType);
        }

        UpdateAllRowsScaleRange();
    }

    private void UpdateAllRowsScaleRange() {
        foreach (var answer in _addedAnswers) {
            if (answer is SurveyAnswerUIEditorLinearScale scaleAnswer) {
                scaleAnswer.SetScaleRange(_min, _max);
            }
        }
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        var answerUI = new SurveyAnswerUIEditorLinearScale(element, index, this, isOther);
        answerUI.SetScaleRange(_min, _max);
        return answerUI;
    }

    public override SurveyAnswerUIBase AddAnswer(string answerText, bool isOther = false) {
        if (_optionsList == null || _answerTemplate == null) {
            Debug.LogWarning("Missing options list or template!");
            return null;
        }

        TemplateContainer element = _answerTemplate.Instantiate();
        int index = _addedAnswers.Count;

        _optionsList.Add(element);

        SurveyAnswerUIBase answerUI = CreateAnswerUI(element, index, isOther);
        if (answerUI is SurveyAnswerUIEditorLinearScale scaleAnswer) {
            scaleAnswer.SetText(answerText);
            scaleAnswer.SetScaleRange(_min, _max);
        }

        _addedAnswers.Add(answerUI);
        RecalculateAnswerIndices();
        return answerUI;
    }

    #region Init
    protected override void RegisterButtons() {
        base.RegisterButtons();

        var addOptionButton = _root.Q<Button>("add-option-button");
        if (addOptionButton != null) {
            addOptionButton.clicked += () => {
                OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
            };
        } else {
            Debug.LogWarning("[RegisterButtons] add-option-button not found in LinearScale");
        }

        RegisterQuestionModalButtonEvents();
    }
    #endregion

    #region Answer Management
    public void AddInitialAnswer() {
        OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
    }

    public void DeleteAnswer(int index) {
        if (index < 0 || index >= _addedAnswers.Count) return;

        var answer = _addedAnswers[index];
        _optionsList.Remove(answer.AnswerElement);
        _addedAnswers.RemoveAt(index);

        RecalculateAnswerIndices();

        OnAnswerRemoved?.Invoke(QuestionID, index);
    }
    #endregion
}
