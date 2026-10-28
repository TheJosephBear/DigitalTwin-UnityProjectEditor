using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurveySystem {
    public class QuestionMultipleChoiceGrid : QuestionGridBase {

        public QuestionMultipleChoiceGrid(int id)
            : base(id, QuestionType.MultipleChoiceGrid) {
            MultipleAnswersAllowed = false;
        }

        protected override AnswerGrid CreateAnswer(int row, int column) {
            return new AnswerGrid(row, column);
        }

    }



    public abstract class QuestionGridBase : QuestionBase {

        public const int MaxRows = 14;
        public const int MaxColumns = 7;

        protected List<string> Rows = new();
        protected List<string> Columns = new();

        protected QuestionGridBase(int id, QuestionType type)
            : base(id, type) {
            IsGrid = true;
        //    AddColumn();
        }

        public void AddRow(string rowText = "") {
            if (Rows.Count >= MaxRows) return;
            Rows.Add(rowText);
            SyncGridAnswers();
        }

        public void AddColumn(string columnText = "") {
            if (Columns.Count >= MaxColumns) return;
            Columns.Add(columnText);
            SyncGridAnswers();
        }

        public string GetRow(int idx) {
            return Rows[idx];
        }

        public string GetColumn(int idx) {
            return Columns[idx];
        }

        public int GetRowCount() {
            return Rows.Count;
        }

        public int GetColumnCount() {
            return Columns.Count;
        }

        public void SetRowText(int idx, string text) {
            Rows[idx] = text;
        }

        public void SetColumnText(int idx, string text) {
            Columns[idx] = text;
        }

        protected void SyncGridAnswers() {
            for (int r = 0; r < Rows.Count; r++) {
                for (int c = 0; c < Columns.Count; c++) {

                    bool exists = _answers.Exists(a =>
                        a is AnswerGrid grid &&
                        grid.Row == r &&
                        grid.Collumn == c
                    );

                    if (!exists) {
                        _answers.Add(CreateAnswer(r, c));
                    }
                }
            }
        }

        public void RemoveRow(int idx) {
            if (Rows.Count <= 1) return;
            if (idx >= 0 && idx < Rows.Count) {
                Rows.RemoveAt(idx);
                _answers.RemoveAll(a => a is AnswerGrid grid && grid.Row == idx);
                foreach (AnswerBase a in _answers) {
                    if (a is AnswerGrid grid && grid.Row > idx) {
                        grid.Row--;
                    }
                }
            }
        }

        public void RemoveColumn(int idx) {
            if (Columns.Count <= 1) return;
            if (idx >= 0 && idx < Columns.Count) {
                Columns.RemoveAt(idx);
                _answers.RemoveAll(a => a is AnswerGrid grid && grid.Collumn == idx);
                foreach (AnswerBase a in _answers) {
                    if (a is AnswerGrid grid && grid.Collumn > idx) {
                        grid.Collumn--;
                    }
                }
            }
        }

        protected abstract AnswerGrid CreateAnswer(int row, int column);

        public override SerializableQuestion Serialize() {
            return new SerializableQuestion {
                Id = Id,
                Title = Title,
                Description = Description,
                IsRequired = IsRequired,
                ViewPointId = ViewPointId,
                ImageId = ImageID,
                QuestionType = QuestionType,
                Rows = new List<string>(Rows),
                Columns = new List<string>(Columns),
                Answers = new List<AnswerBase>(_answers)
            };
        }

        public override QuestionBase Deserialize(SerializableQuestion serializable) {
            Title = serializable.Title;
            Description = serializable.Description;
            IsRequired = serializable.IsRequired;
            ViewPointId = serializable.ViewPointId;
            ImageID = serializable.ImageId;
            Rows = serializable.Rows != null ? serializable.Rows.ConvertAll(r => ClearLegacyDefault(r, "Row")) : new List<string>();
            Columns = serializable.Columns != null ? serializable.Columns.ConvertAll(c => ClearLegacyDefault(c, "Column")) : new List<string>();
            CropToLimit(Rows, MaxRows, "rows");
            CropToLimit(Columns, MaxColumns, "columns");
            SyncGridAnswers();

            return this;
        }

        // Surveys saved before the limits existed can have more rows/columns, the extra ones are dropped
        private void CropToLimit(List<string> items, int limit, string what) {
            if (items.Count <= limit) return;
            Debug.LogWarning($"[Survey] Question {Id} has {items.Count} {what}, keeping the first {limit}.");
            items.RemoveRange(limit, items.Count - limit);
        }

        // Older surveys stored "Row"/"Column" as the default text, which hid the editor placeholder
        private static string ClearLegacyDefault(string text, string legacyDefault) {
            return text == legacyDefault ? string.Empty : text;
        }

    }

}
