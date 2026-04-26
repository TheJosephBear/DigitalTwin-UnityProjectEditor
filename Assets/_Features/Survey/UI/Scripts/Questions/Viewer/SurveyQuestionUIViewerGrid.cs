using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIViewerGrid : SurveyQuestionUIViewer {

    private List<SurveyAnswerUIViewerGrid> _rows = new();
    private List<string> _columnTexts = new();

    private VisualElement _rowContainer;
    private VisualElement _columnContainer;

    public event Action<int, int, int, bool> OnGridAnswerSelected; // qId, rowIdx, colIdx, value

    public SurveyQuestionUIViewerGrid(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

        _rowContainer = _root.Q<VisualElement>("options-list");
        _columnContainer = _root.Q<VisualElement>("col-headers");

        _rowContainer?.Clear();
        _columnContainer?.Clear();
    }

    public void AddColumn(string text) {
        _columnTexts.Add(text);

        var label = new Label(text);
        label.style.width = 100f; // Match editor's base width
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        _columnContainer.Add(label);

        // Whenever a column is added, rows must update their radio button counts
        RefreshRows();
    }

    public void AddRow(string text) {
        if (_answerTemplate == null) return;

        var element = _answerTemplate.Instantiate();
        var rowUI = new SurveyAnswerUIViewerGrid(element, _rows.Count, this);
        rowUI.SetRowText(text);

        _rows.Add(rowUI);
        _rowContainer.Add(element);

        rowUI.RebuildRadioButtons(_columnTexts.Count, _questionType == QuestionType.CheckboxGrid);
    }

    private void RefreshRows() {
        foreach (var row in _rows) {
            row.RebuildRadioButtons(_columnTexts.Count, _questionType == QuestionType.CheckboxGrid);
        }
    }

    public void InvokeAnswerSelected(int rowIdx, int colIdx, bool value) {
        OnGridAnswerSelected?.Invoke(QuestionID, rowIdx, colIdx, value);
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) => null;
    protected override void RegisterButtons() { }
    protected override void RegisterDropdown() { }
    protected override void RegisterTextInputs() { }

    public override void AddAnswer(string answerText, bool isOther = false) {
        
    }
}
