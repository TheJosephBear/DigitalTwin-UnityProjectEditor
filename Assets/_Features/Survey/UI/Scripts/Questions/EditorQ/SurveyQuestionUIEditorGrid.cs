using SurveySystem;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
public class SurveyQuestionUIEditorGrid : SurveyQuestionUIEditor {

    private List<SurveyAnswerUIEditorGrid> _rows = new();
    private List<SurveyAnswerUIEditorGrid> _columns = new();

    private VisualElement _rowContainer;
    private VisualElement _columnContainer; 

    #region Events

    public event Action<int, SurveyAnswerUIEditorGrid> OnAddRow;
    public event Action<int, SurveyAnswerUIEditorGrid> OnAddColumn;

    #endregion

    public SurveyQuestionUIEditorGrid(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder,
        bool isDeserialized = false) 
        : base(root, questionId, questionType, viewPoints, uiBuilder){
        
        _rowContainer = _root.Q<VisualElement>("options-list");
        _columnContainer = _root.Q<VisualElement>("col-headers");

        _rowContainer.Clear();
        _columnContainer.Clear();
        RebuildGrid();
        AddInitialAnswer();
    }


    public void AddInitialAnswer() {

    }

    protected override void RegisterButtons() {
        base.RegisterButtons();

        _rowContainer = _root.Q<VisualElement>("options-list");
        _columnContainer = _root.Q<VisualElement>("col-headers");

        var addRowButton = _root.Q<Button>("add-row-button");
        if (addRowButton != null) {
            addRowButton.clicked += () =>
            {
                OnAddRow?.Invoke(QuestionID, AddRow());
            };
        }

        var addColumnButton = _root.Q<Button>("add-column-button");
        if (addColumnButton != null) {
            addColumnButton.clicked += () =>
            {
                OnAddColumn?.Invoke(QuestionID, AddColumn());
            };
        }

        RegisterQuestionModalButtonEvents();
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return new SurveyAnswerUIEditorGrid(element, index, this, isOther);
    }

    public void AddExistingRow(string text) {
        AddRow().SetText(text);
    }

    public void AddExistingColumn(string text) {
        AddColumn().SetText(text);
    }

    public SurveyAnswerUIEditorGrid AddRow() {
        if (_answerTemplate == null) {
            Debug.LogError("Answer template is null!");
        }
        var element = _answerTemplate.Instantiate();
        _rowContainer.Add(element);

        SurveyAnswerUIEditorGrid row = new SurveyAnswerUIEditorGrid(element, _rows.Count, this, false);
        _rows.Add(row);

        RebuildGrid();
        return row;
    }

    public SurveyAnswerUIEditorGrid AddColumn() {
        if (_answerTemplate == null) {
            Debug.LogError("Answer template is null!");
        }
        var element = _surveyUIBuilder.GridCollumnTemplate.CloneTree(); // CreateTextField();
        _columnContainer.Add(element);

        SurveyAnswerUIEditorGrid column = new SurveyAnswerUIEditorGrid(element, _columns.Count, this, false);
        _columns.Add(column);

        RebuildGrid();
        return column;
    }

    private void RebuildGrid() {
        foreach (var row in _rows) {
            row.RebuildRadioButtons(_columns.Count, _questionType == QuestionType.CheckboxGrid);
        }
    }

    TextField CreateTextField() {
        var textField = new TextField();
        textField.value = "Sloupec";

        float baseWidth = 100f; // minimum width

        textField.style.width = baseWidth;

        // Register change
        textField.RegisterValueChangedCallback(evt =>
        {
            var textElement = textField.Q<TextElement>();

            if (textElement == null) return;

            var size = textElement.MeasureTextSize(
                evt.newValue,
                0,
                VisualElement.MeasureMode.Undefined,
                0,
                VisualElement.MeasureMode.Undefined
            );

            float newWidth = Mathf.Max(baseWidth, size.x + 20f); // padding buffer

            textField.style.width = newWidth;
        });
        return textField;
    }
}