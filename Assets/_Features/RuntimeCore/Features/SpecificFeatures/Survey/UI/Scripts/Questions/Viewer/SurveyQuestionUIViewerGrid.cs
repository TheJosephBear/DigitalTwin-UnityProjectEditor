using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Matrix is built from plain flex rows instead of a MultiColumnListView: header and cells share the same
// column sizing (so checkboxes line up under their column titles), texts wrap instead of being cut
// with "..." and the table grows with its content instead of scrolling inside a fixed-size box.
public class SurveyQuestionUIViewerGrid : SurveyQuestionUIViewer {

    private const string LabelColClass = "grid-table__label-col";
    private const string OptionColClass = "grid-table__option-col";

    // Row-title column fits its longest title, but never takes the answer columns' space
    private const float LabelColMinWidth = 70f;
    private const float LabelColMaxShare = 0.3f;
    private const float LabelColExtraWidth = 20f; // Label paddings

    private List<string> _rowTexts = new();
    private List<string> _columnTexts = new();

    private Dictionary<int, int> _selectedColPerRow = new(); // rowIdx -> colIdx
    private HashSet<(int row, int col)> _checkedCells = new(); // (rowIdx, colIdx)

    private VisualElement _table;
    private VisualElement _header;
    private VisualElement _corner;
    private List<Label> _rowLabels = new();
    private List<VisualElement> _rowElements = new();
    private Dictionary<(int row, int col), Toggle> _cellToggles = new();

    public int ColumnCount => _columnTexts.Count;

    public event Action<int, int, int, bool> OnGridAnswerSelected; // qId, rowIdx, colIdx, value

    public SurveyQuestionUIViewerGrid(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

        InitializeTable();
    }

    private void InitializeTable() {
        _table = _root.Q<VisualElement>("grid-table");
        if (_table == null) return;

        _table.Clear();
        _table.AddToClassList("grid-table");

        _header = new VisualElement();
        _header.AddToClassList("grid-table__header");

        _corner = new VisualElement();
        _corner.AddToClassList(LabelColClass);
        _header.Add(_corner);

        _table.Add(_header);
        _table.RegisterCallback<GeometryChangedEvent>(_ => UpdateLabelColumnWidth());
    }

    private void UpdateLabelColumnWidth() {
        float tableWidth = _table.layout.width;
        if (float.IsNaN(tableWidth) || tableWidth <= 0f) return;

        float longest = 0f;
        foreach (Label label in _rowLabels) {
            if (string.IsNullOrEmpty(label.text)) continue;
            float textWidth = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x;
            if (!float.IsNaN(textWidth)) longest = Mathf.Max(longest, textWidth);
        }

        float width = Mathf.Clamp(longest + LabelColExtraWidth, LabelColMinWidth, Mathf.Max(LabelColMinWidth, tableWidth * LabelColMaxShare));
        width = Mathf.Ceil(width);
        if (Mathf.Approximately(_corner.resolvedStyle.width, width)) return; // Avoid relayout loops

        _corner.style.width = width;
        foreach (Label label in _rowLabels) label.style.width = width;
    }

    public void AddColumn(string text) {
        int colIdx = _columnTexts.Count;
        _columnTexts.Add(text);
        if (_table == null) return;

        var title = new Label(text);
        title.AddToClassList(OptionColClass);
        title.AddToClassList("grid-table__col-title");
        _header.Add(title);

        // Rows added before this column need a cell for it too
        for (int rowIdx = 0; rowIdx < _rowElements.Count; rowIdx++) {
            _rowElements[rowIdx].Add(CreateCell(rowIdx, colIdx));
        }
    }

    public void AddRow(string text) {
        int rowIdx = _rowTexts.Count;
        _rowTexts.Add(text);
        if (_table == null) return;

        var row = new VisualElement();
        row.AddToClassList("grid-table__row");

        var label = new Label(text) { name = "row-label" };
        label.AddToClassList(LabelColClass);
        label.AddToClassList("grid-table__row-title");
        row.Add(label);
        _rowLabels.Add(label);

        for (int colIdx = 0; colIdx < _columnTexts.Count; colIdx++) {
            row.Add(CreateCell(rowIdx, colIdx));
        }

        _rowElements.Add(row);
        _table.Add(row);
    }

    private VisualElement CreateCell(int rowIdx, int colIdx) {
        var cell = new VisualElement();
        cell.AddToClassList(OptionColClass);
        cell.AddToClassList("grid-table__cell");

        bool isCheckbox = _questionType == QuestionType.CheckboxGrid;
        var toggle = new Toggle { name = isCheckbox ? "grid-cell-toggle" : "grid-cell-radio" };
        toggle.AddToClassList(isCheckbox ? "grid-checkbox-toggle" : "grid-radio-toggle");
        toggle.text = string.Empty;
        toggle.label = string.Empty;
        cell.Add(toggle);
        _cellToggles[(rowIdx, colIdx)] = toggle;

        toggle.RegisterValueChangedCallback(evt => {
            if (isCheckbox) {
                if (evt.newValue) _checkedCells.Add((rowIdx, colIdx));
                else _checkedCells.Remove((rowIdx, colIdx));
                InvokeAnswerSelected(rowIdx, colIdx, evt.newValue);
            } else if (evt.newValue) {
                SelectRadio(rowIdx, colIdx);
            } else {
                // Clicking the already selected radio keeps it selected
                toggle.SetValueWithoutNotify(true);
            }
        });

        // Clicking anywhere in the cell acts like clicking the toggle itself
        cell.RegisterCallback<ClickEvent>(evt => {
            if (evt.target == toggle || (evt.target as VisualElement)?.GetFirstAncestorOfType<Toggle>() == toggle) return;
            toggle.value = isCheckbox ? !toggle.value : true;
        });

        return cell;
    }

    private void SelectRadio(int rowIdx, int colIdx) {
        if (_selectedColPerRow.TryGetValue(rowIdx, out int prevCol) && prevCol != colIdx
            && _cellToggles.TryGetValue((rowIdx, prevCol), out var prevToggle)) {
            prevToggle.SetValueWithoutNotify(false);
        }
        _selectedColPerRow[rowIdx] = colIdx;
        InvokeAnswerSelected(rowIdx, colIdx, true);
    }

    public override void SetValidationError(bool invalid, QuestionValidationResult result) {
        base.SetValidationError(invalid, result);
        for (int i = 0; i < _rowElements.Count; i++) {
            bool rowInvalid = invalid && result != null && result.MissingRows.Contains(i);
            _rowElements[i].EnableInClassList("grid-row--invalid", rowInvalid);
        }
    }

    public void InvokeAnswerSelected(int rowIdx, int colIdx, bool value) {
        OnGridAnswerSelected?.Invoke(QuestionID, rowIdx, colIdx, value);
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) => null;
    protected override void RegisterButtons() {
        base.RegisterButtons();
    }
    protected override void RegisterDropdown() { }
    protected override void RegisterTextInputs() { }

    public override SurveyAnswerUIBase AddAnswer(string answerText, bool isOther = false) {
        return null;
    }
}
