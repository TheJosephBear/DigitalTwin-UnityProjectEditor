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
    private VisualElement _gridContainer;

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
        
    }


    public void AddInitialAnswer() {

    }

    protected override void RegisterButtons() {
        _rowContainer = _root.Q<VisualElement>("option-container");
        _columnContainer = _root.Q<VisualElement>("col-headers");
        _gridContainer = _root.Q<VisualElement>("grid-container");

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
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return new SurveyAnswerUIEditorGrid(element, index, this, isOther);
    }

    SurveyAnswerUIEditorGrid AddRow() {
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

    SurveyAnswerUIEditorGrid AddColumn() {
        if (_answerTemplate == null) {
            Debug.LogError("Answer template is null!");
        }
        var element = _answerTemplate.Instantiate(); ;
        _columnContainer.Add(element);

        SurveyAnswerUIEditorGrid column = new SurveyAnswerUIEditorGrid(element, _columns.Count, this, false);
        _columns.Add(column);

        RebuildGrid();
        return column;
    }

    private void RebuildGrid() {
        _gridContainer.Clear();

        int rowCount = _rows.Count;
        int colCount = _columns.Count;

        for (int r = 0; r < rowCount; r++) {
            var rowElement = new VisualElement();
            rowElement.style.flexDirection = FlexDirection.Row;

            for (int c = 0; c < colCount; c++) {
                var cell = new VisualElement();
                cell.style.width = 20;
                cell.style.height = 20;

                // Placeholder (toggle, checkbox, radio, etc.)
                cell.AddToClassList("grid-cell");

                rowElement.Add(cell);
            }

            _gridContainer.Add(rowElement);
        }
    }
}
