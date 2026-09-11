using SurveySystem;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UIRadioButton = UnityEngine.UIElements.RadioButton;

public class RadioCallbacks {
    public EventCallback<PointerDownEvent> PointerCb;
    public EventCallback<ClickEvent> ClickCb;
    public EventCallback<ChangeEvent<bool>> ChangeCb;
}

public class SurveyQuestionUIEditorGrid : SurveyQuestionUIEditor {

    private List<SurveyAnswerUIEditorGrid> _rows = new();
    private List<SurveyAnswerUIEditorGrid> _columns = new();
    private List<int> _rowIndices = new();

    private Dictionary<int, int> _selectedColPerRow = new(); // rowIdx -> colIdx

    private MultiColumnListView _table;

    private void RefreshAllRowRadioButtons() {
        if (_table == null) return;
        for (int i = 0; i < _rows.Count; i++) {
            _table.RefreshItem(i);
        }
    }

    #region Events

    public event Action<int, SurveyAnswerUIEditorGrid> OnAddRow;
    public event Action<int, SurveyAnswerUIEditorGrid> OnAddColumn;
    public event Action<int, int> OnRemoveRow; // questionID, rowIdx
    public event Action<int, int> OnRemoveColumn; // questionID, colIdx

    #endregion

    public SurveyQuestionUIEditorGrid(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder,
        bool isDeserialized = false) 
        : base(root, questionId, questionType, viewPoints, uiBuilder){
        
        InitializeTable();
        RebuildGrid();
    }

    private void InitializeTable() {
        _table = _root.Q<MultiColumnListView>("grid-table") ?? _root.Q<MultiColumnListView>();
        if (_table == null) return;

        _table.fixedItemHeight = 44f;
        _table.reorderable = false;
        _table.showAddRemoveFooter = false;
        _table.selectionType = SelectionType.None;
        _table.itemsSource = _rowIndices;

        RebuildTableColumns();
    }

    private void RebuildTableColumns() {
        if (_table == null) return;
        _table.columns.Clear();

        // 1. Column 0: Row Title Column
        var rowTitleCol = CreateRowTitleColumn();
        _table.columns.Add(rowTitleCol);

        // 2. Columns 1..N: Option Columns
        for (int i = 0; i < _columns.Count; i++) {
            var col = CreateOptionColumn(i);
            _table.columns.Add(col);
        }

        // 3. Last Column: Row Actions Column (move up, move down, delete row)
        var rowActionsCol = CreateRowActionsColumn();
        _table.columns.Add(rowActionsCol);
    }

    private Column CreateRowTitleColumn() {
        var rowTitleCol = new Column {
            name = "row-title-col",
            title = "Řádky / Sloupce",
            width = 150,
            minWidth = 120,
            stretchable = false
        };

        rowTitleCol.makeCell = () => {
            var container = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    flexGrow = 1,
                    width = Length.Percent(100)
                }
            };

            var textField = new TextField { name = "row-title" };
            textField.multiline = true;
            textField.style.flexGrow = 1;
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.style.color = new Color(0, 0, 0);
            textField.style.width = Length.Percent(100);

            container.Add(textField);
            return container;
        };

        rowTitleCol.bindCell = (VisualElement cell, int rowIndex) => {
            if (rowIndex < 0 || rowIndex >= _rows.Count) return;

            var textField = cell.Q<TextField>("row-title");
            if (textField != null) {
                var rowUI = _rows[rowIndex];
                rowUI.UpdateIndex(rowIndex);
                textField.SetValueWithoutNotify(rowUI.Text);

                if (textField.userData is EventCallback<ChangeEvent<string>> oldCb) {
                    textField.UnregisterValueChangedCallback(oldCb);
                }
                EventCallback<ChangeEvent<string>> newCb = evt => {
                    if (rowIndex < _rows.Count) {
                        _rows[rowIndex].SetText(evt.newValue);
                        _rows[rowIndex].InvokeTextChanged(rowIndex, evt.newValue);
                    }
                };
                textField.userData = newCb;
                textField.RegisterValueChangedCallback(newCb);

                textField.RegisterCallback<FocusOutEvent>(evt => {
                    if (rowIndex < _rows.Count && textField != null) {
                        _rows[rowIndex].SetText(textField.value);
                        _rows[rowIndex].InvokeTextChanged(rowIndex, textField.value);
                    }
                });
            }
        };

        return rowTitleCol;
    }

    private Column CreateOptionColumn(int colIdx) {
        var col = new Column {
            name = $"col-{colIdx}",
            title = $"Sloupec {colIdx + 1}",
            width = 100,
            minWidth = 80,
            stretchable = true
        };

        col.makeHeader = () => {
            var header = new VisualElement {
                name = "grid-column-header-container",
                style = {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    flexGrow = 1,
                    width = Length.Percent(100),
                    alignSelf = Align.Stretch
                }
            };

            var deleteBtn = new Button { name = "col-delete-button" };
            deleteBtn.AddToClassList("icon-trash");
            deleteBtn.style.marginBottom = 2;

            var tf = new TextField { name = "column-title" };
            tf.multiline = true;
            tf.style.whiteSpace = WhiteSpace.Normal;
            tf.style.color = new Color(0, 0, 0);
            tf.style.flexGrow = 1;
            tf.style.width = Length.Percent(100);
            tf.style.alignSelf = Align.Stretch;
            tf.style.unityTextAlign = TextAnchor.MiddleCenter;

            header.Add(deleteBtn);
            header.Add(tf);
            return header;
        };

        col.bindHeader = (VisualElement header) => {
            int currentColIdx = colIdx;
            var tf = header.Q<TextField>("column-title");
            if (tf != null && currentColIdx < _columns.Count) {
                var colUI = _columns[currentColIdx];
                colUI.UpdateIndex(currentColIdx);
                tf.SetValueWithoutNotify(colUI.Text);

                if (tf.userData is EventCallback<ChangeEvent<string>> oldCb) {
                    tf.UnregisterValueChangedCallback(oldCb);
                }
                EventCallback<ChangeEvent<string>> newCb = evt => {
                    if (currentColIdx < _columns.Count) {
                        _columns[currentColIdx].SetText(evt.newValue);
                        _columns[currentColIdx].InvokeTextChanged(currentColIdx, evt.newValue);
                    }
                };
                tf.userData = newCb;
                tf.RegisterValueChangedCallback(newCb);

                tf.RegisterCallback<FocusOutEvent>(evt => {
                    if (currentColIdx < _columns.Count && tf != null) {
                        _columns[currentColIdx].SetText(tf.value);
                        _columns[currentColIdx].InvokeTextChanged(currentColIdx, tf.value);
                    }
                });
            }

            var deleteBtn = header.Q<Button>("col-delete-button");
            if (deleteBtn != null) {
                deleteBtn.clickable = new Clickable(() => RemoveColumn(currentColIdx));
            }
        };

        col.makeCell = () => {
            var cell = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    flexGrow = 1,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };
            cell.AddToClassList("grid-cell-container");
            if (_questionType == QuestionType.CheckboxGrid) {
                cell.Add(new Toggle { name = "grid-cell-toggle" });
            } else {
                cell.Add(new CustomRadioButtonNoText { name = "grid-cell-radio" });
            }
            cell.RegisterCallback<ClickEvent>(evt => {
                var toggle = cell.Q<Toggle>("grid-cell-toggle");
                if (toggle != null) {
                    if (evt.target != toggle && (evt.target as VisualElement)?.GetFirstAncestorOfType<Toggle>() != toggle) {
                        toggle.value = !toggle.value;
                    }
                }
                var radio = cell.Q<CustomRadioButtonNoText>("grid-cell-radio");
                if (radio != null && radio.Radio != null) {
                    if (evt.target != radio && (evt.target as VisualElement)?.GetFirstAncestorOfType<CustomRadioButtonNoText>() != radio) {
                        if (!radio.Radio.value) {
                            radio.Radio.value = true;
                        }
                    }
                }
            });
            return cell;
        };

        col.bindCell = (VisualElement cell, int rowIndex) => {
            if (rowIndex < 0 || rowIndex >= _rows.Count) return;
            int currentColIdx = colIdx;
            if (_questionType == QuestionType.CheckboxGrid) {
                var toggle = cell.Q<Toggle>("grid-cell-toggle");
                if (toggle != null) {
                    // Option cell toggle binding
                }
            } else {
                var radio = cell.Q<CustomRadioButtonNoText>("grid-cell-radio");
                if (radio != null) {
                    bool isSelected = _selectedColPerRow.TryGetValue(rowIndex, out int selCol) && selCol == currentColIdx;
                    radio.Radio.SetValueWithoutNotify(isSelected);

                    if (radio.userData is RadioCallbacks oldCbs) {
                        if (oldCbs.PointerCb != null) radio.UnregisterCallback(oldCbs.PointerCb, TrickleDown.TrickleDown);
                        if (oldCbs.ClickCb != null) radio.UnregisterCallback(oldCbs.ClickCb, TrickleDown.TrickleDown);
                        if (oldCbs.ChangeCb != null) radio.Radio.UnregisterValueChangedCallback(oldCbs.ChangeCb);
                    }

                    EventCallback<PointerDownEvent> pointerCb = evt => {
                        _selectedColPerRow[rowIndex] = currentColIdx;
                        RefreshAllRowRadioButtons();
                        evt.StopPropagation();
                    };
                    EventCallback<ClickEvent> clickCb = evt => {
                        _selectedColPerRow[rowIndex] = currentColIdx;
                        RefreshAllRowRadioButtons();
                        evt.StopPropagation();
                    };
                    EventCallback<ChangeEvent<bool>> changeCb = evt => {
                        if (evt.newValue) {
                            _selectedColPerRow[rowIndex] = currentColIdx;
                            RefreshAllRowRadioButtons();
                        }
                    };

                    radio.userData = new RadioCallbacks {
                        PointerCb = pointerCb,
                        ClickCb = clickCb,
                        ChangeCb = changeCb
                    };

                    radio.RegisterCallback(pointerCb, TrickleDown.TrickleDown);
                    radio.RegisterCallback(clickCb, TrickleDown.TrickleDown);
                    radio.Radio.RegisterValueChangedCallback(changeCb);
                }
            }
        };

        return col;
    }

    private Column CreateRowActionsColumn() {
        var actionsCol = new Column {
            name = "row-actions-col",
            title = "",
            width = 110,
            minWidth = 90,
            stretchable = false
        };

        actionsCol.makeHeader = () => {
            return new VisualElement {
                style = {
                    flexGrow = 1,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };
        };

        actionsCol.makeCell = () => {
            var container = new VisualElement {
                name = "edit-option-container",
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    flexGrow = 1,
                    width = Length.Percent(100)
                }
            };

            var moveUpBtn = new Button { name = "row-move-up-button" };
            moveUpBtn.AddToClassList("icon-move-up");

            var moveDownBtn = new Button { name = "row-move-down-button" };
            moveDownBtn.AddToClassList("icon-move-down");

            var deleteBtn = new Button { name = "row-delete-button" };
            deleteBtn.AddToClassList("icon-trash");

            container.Add(moveUpBtn);
            container.Add(moveDownBtn);
            container.Add(deleteBtn);
            return container;
        };

        actionsCol.bindCell = (VisualElement cell, int rowIndex) => {
            if (rowIndex < 0 || rowIndex >= _rows.Count) return;
            int currentIdx = rowIndex;

            var moveUpBtn = cell.Q<Button>("row-move-up-button");
            if (moveUpBtn != null) {
                moveUpBtn.clickable = new Clickable(() => MoveRowUp(currentIdx));
                moveUpBtn.SetEnabled(currentIdx > 0);
            }

            var moveDownBtn = cell.Q<Button>("row-move-down-button");
            if (moveDownBtn != null) {
                moveDownBtn.clickable = new Clickable(() => MoveRowDown(currentIdx));
                moveDownBtn.SetEnabled(currentIdx < _rows.Count - 1);
            }

            var deleteBtn = cell.Q<Button>("row-delete-button");
            if (deleteBtn != null) {
                deleteBtn.clickable = new Clickable(() => RemoveRow(currentIdx));
            }
        };

        return actionsCol;
    }

    protected override void RegisterButtons() {
        base.RegisterButtons();

        var addRowButton = _root.Q<Button>("add-row-button");
        if (addRowButton != null) {
            addRowButton.clickable = new Clickable(() =>
            {
                if (_rows.Count < 20) {
                    var newRow = AddRow();
                    if (newRow != null) {
                        OnAddRow?.Invoke(QuestionID, newRow);
                    }
                }
            });
        }

        var addColumnButton = _root.Q<Button>("add-column-button");
        if (addColumnButton != null) {
            addColumnButton.clickable = new Clickable(() =>
            {
                if (_columns.Count < 8) {
                    var newCol = AddColumn();
                    if (newCol != null) {
                        OnAddColumn?.Invoke(QuestionID, newCol);
                    }
                }
            });
        }

        RegisterQuestionModalButtonEvents();
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return new SurveyAnswerUIEditorGrid(element, index, this, isOther);
    }

    public void AddExistingRow(string text) {
        AddRow(text);
    }

    public void AddExistingColumn(string text) {
        AddColumn(text);
    }

    public SurveyAnswerUIEditorGrid AddRow(string initialText = "") {
        if (_rows.Count >= 20) return null;

        var element = _answerTemplate != null ? _answerTemplate.Instantiate() : new VisualElement();
        SurveyAnswerUIEditorGrid row = new SurveyAnswerUIEditorGrid(element, _rows.Count, this, false);
        if (!string.IsNullOrEmpty(initialText)) {
            row.SetText(initialText);
        }
        _rows.Add(row);
        _rowIndices.Add(_rows.Count - 1);

        SyncAnswerIndices();
        RebuildGrid();
        return row;
    }

    public SurveyAnswerUIEditorGrid AddColumn(string initialText = "") {
        if (_columns.Count >= 8) return null;

        var element = _surveyUIBuilder.GridCollumnTemplate != null ? _surveyUIBuilder.GridCollumnTemplate.CloneTree() : new VisualElement();
        int colIdx = _columns.Count;

        SurveyAnswerUIEditorGrid column = new SurveyAnswerUIEditorGrid(element, colIdx, this, false);
        if (!string.IsNullOrEmpty(initialText)) {
            column.SetText(initialText);
        }
        _columns.Add(column);

        SyncAnswerIndices();
        RebuildTableColumns();
        RebuildGrid();
        return column;
    }

    public void RemoveColumn(int colIdx) {
        if (_columns.Count <= 1) return;
        if (colIdx < 0 || colIdx >= _columns.Count) return;
        _columns.RemoveAt(colIdx);
        RebuildTableColumns();
        RebuildGrid();
        OnRemoveColumn?.Invoke(QuestionID, colIdx);
    }

    public void RemoveRow(int rowIdx) {
        if (_rows.Count <= 1) return;
        if (rowIdx < 0 || rowIdx >= _rows.Count) return;
        _rows.RemoveAt(rowIdx);
        _rowIndices.Clear();
        for (int i = 0; i < _rows.Count; i++) _rowIndices.Add(i);
        RebuildGrid();
        OnRemoveRow?.Invoke(QuestionID, rowIdx);
    }

    public void MoveRowUp(int rowIdx) {
        if (rowIdx <= 0 || rowIdx >= _rows.Count) return;
        var temp = _rows[rowIdx];
        _rows[rowIdx] = _rows[rowIdx - 1];
        _rows[rowIdx - 1] = temp;
        RebuildGrid();
    }

    public void MoveRowDown(int rowIdx) {
        if (rowIdx < 0 || rowIdx >= _rows.Count - 1) return;
        var temp = _rows[rowIdx];
        _rows[rowIdx] = _rows[rowIdx + 1];
        _rows[rowIdx + 1] = temp;
        RebuildGrid();
    }

    private void SyncAnswerIndices() {
        for (int i = 0; i < _rows.Count; i++) {
            _rows[i].UpdateIndex(i);
        }
        for (int i = 0; i < _columns.Count; i++) {
            _columns[i].UpdateIndex(i);
        }
    }

    private void RebuildGrid() {
        SyncAnswerIndices();
        if (_table != null) {
            _table.itemsSource = _rowIndices;
            _table.Rebuild();
            _table.RefreshItems();
        }
        foreach (var row in _rows) {
            row.RebuildRadioButtons(_columns.Count, _questionType == QuestionType.CheckboxGrid);
        }
    }
}