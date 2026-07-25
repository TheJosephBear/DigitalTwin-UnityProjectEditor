using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIViewerGrid : SurveyQuestionUIViewer {

    private List<string> _rowTexts = new();
    private List<string> _columnTexts = new();
    private List<int> _rowIndices = new();

    private Dictionary<int, int> _selectedColPerRow = new(); // rowIdx -> colIdx
    private HashSet<(int row, int col)> _checkedCells = new(); // (rowIdx, colIdx)

    private MultiColumnListView _table;

    public event Action<int, int, int, bool> OnGridAnswerSelected; // qId, rowIdx, colIdx, value

    public SurveyQuestionUIViewerGrid(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

        InitializeTable();
    }

    private void InitializeTable() {
        _table = _root.Q<MultiColumnListView>("grid-table") ?? _root.Q<MultiColumnListView>();
        if (_table == null) return;

        _table.fixedItemHeight = 40f;
        _table.reorderable = false;
        _table.showAddRemoveFooter = false;
        _table.selectionType = SelectionType.None;
        _table.columns.Clear();
        _table.itemsSource = _rowIndices;

        var rowTitleCol = new Column {
            name = "row-title-col",
            title = "",
            width = 160,
            minWidth = 120,
            stretchable = false
        };

        rowTitleCol.makeCell = () => {
            var container = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexStart,
                    flexGrow = 1,
                    width = Length.Percent(100)
                }
            };
            var label = new Label { name = "row-label" };
            label.style.flexGrow = 1;
            label.style.whiteSpace = WhiteSpace.Normal;
            container.Add(label);
            return container;
        };

        rowTitleCol.bindCell = (VisualElement cell, int rowIndex) => {
            if (rowIndex < 0 || rowIndex >= _rowTexts.Count) return;
            var label = cell.Q<Label>("row-label");
            if (label != null) {
                label.text = _rowTexts[rowIndex];
            }
        };

        _table.columns.Add(rowTitleCol);
    }

    private void RefreshAllRowRadioButtons() {
        if (_table == null) return;
        for (int i = 0; i < _rowIndices.Count; i++) {
            _table.RefreshItem(i);
        }
    }

    public void AddColumn(string text) {
        int colIdx = _columnTexts.Count;
        _columnTexts.Add(text);

        if (_table != null) {
            var col = new Column {
                name = $"col-{colIdx}",
                title = text,
                width = 100,
                minWidth = 80,
                stretchable = true
            };

            col.makeCell = () => {
                var cellContainer = new VisualElement {
                    style = {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        justifyContent = Justify.Center,
                        flexGrow = 1,
                        width = Length.Percent(100)
                    }
                };
                if (_questionType == QuestionType.CheckboxGrid) {
                    var toggle = new Toggle { name = "grid-cell-toggle" };
                    cellContainer.Add(toggle);
                } else {
                    var radio = new CustomRadioButtonNoText { name = "grid-cell-radio" };
                    cellContainer.Add(radio);
                }
                return cellContainer;
            };

            col.bindCell = (VisualElement cell, int rowIndex) => {
                if (rowIndex < 0 || rowIndex >= _rowIndices.Count) return;
                if (_questionType == QuestionType.CheckboxGrid) {
                    var toggle = cell.Q<Toggle>("grid-cell-toggle");
                    if (toggle != null) {
                        bool isChecked = _checkedCells.Contains((rowIndex, colIdx));
                        toggle.SetValueWithoutNotify(isChecked);
                        if (toggle.userData is EventCallback<ChangeEvent<bool>> oldCb) {
                            toggle.UnregisterValueChangedCallback(oldCb);
                        }
                        EventCallback<ChangeEvent<bool>> newCb = evt => {
                            if (evt.newValue) {
                                _checkedCells.Add((rowIndex, colIdx));
                            } else {
                                _checkedCells.Remove((rowIndex, colIdx));
                            }
                            InvokeAnswerSelected(rowIndex, colIdx, evt.newValue);
                        };
                        toggle.userData = newCb;
                        toggle.RegisterValueChangedCallback(newCb);
                    }
                } else {
                    var radio = cell.Q<CustomRadioButtonNoText>("grid-cell-radio");
                    if (radio != null) {
                        bool isSelected = _selectedColPerRow.TryGetValue(rowIndex, out int selCol) && selCol == colIdx;
                        radio.Radio.SetValueWithoutNotify(isSelected);

                        if (radio.userData is RadioCallbacks oldCbs) {
                            if (oldCbs.PointerCb != null) radio.UnregisterCallback(oldCbs.PointerCb, TrickleDown.TrickleDown);
                            if (oldCbs.ClickCb != null) radio.UnregisterCallback(oldCbs.ClickCb, TrickleDown.TrickleDown);
                            if (oldCbs.ChangeCb != null) radio.Radio.UnregisterValueChangedCallback(oldCbs.ChangeCb);
                        }

                        EventCallback<PointerDownEvent> pointerCb = evt => {
                            _selectedColPerRow[rowIndex] = colIdx;
                            InvokeAnswerSelected(rowIndex, colIdx, true);
                            RefreshAllRowRadioButtons();
                            evt.StopPropagation();
                        };
                        EventCallback<ClickEvent> clickCb = evt => {
                            _selectedColPerRow[rowIndex] = colIdx;
                            InvokeAnswerSelected(rowIndex, colIdx, true);
                            RefreshAllRowRadioButtons();
                            evt.StopPropagation();
                        };
                        EventCallback<ChangeEvent<bool>> changeCb = evt => {
                            if (evt.newValue) {
                                _selectedColPerRow[rowIndex] = colIdx;
                                InvokeAnswerSelected(rowIndex, colIdx, true);
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

            _table.columns.Add(col);
            _table.Rebuild();
            _table.RefreshItems();
        }
    }

    public void AddRow(string text) {
        _rowTexts.Add(text);
        _rowIndices.Add(_rowTexts.Count - 1);
        if (_table != null) {
            _table.itemsSource = _rowIndices;
            _table.Rebuild();
            _table.RefreshItems();
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
