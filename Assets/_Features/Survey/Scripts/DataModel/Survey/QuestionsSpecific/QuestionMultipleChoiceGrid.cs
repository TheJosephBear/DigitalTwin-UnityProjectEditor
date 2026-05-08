using QuestionnaireToolkit.Scripts.SimpleJSON;
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

        protected List<string> Rows = new();
        protected List<string> Columns = new();

        protected QuestionGridBase(int id, QuestionType type)
            : base(id, type) {
            IsGrid = true;
        //    AddColumn();
        }

        public void AddRow(string rowText = "Row") {
            Rows.Add(rowText);
            SyncGridAnswers();
        }

        public void AddColumn(string columnText = "Column") {
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

        protected abstract AnswerGrid CreateAnswer(int row, int column);

        public override SerializableQuestion Serialize() {
            Debug.Log("Correct serialize");
            Debug.Log(Rows.Count);
            return new SerializableGridQuestion {
                Id = Id,
                Title = Title,
                Description = Description,
                ViewPointId = ViewPointId,
                QuestionType = QuestionType,
                Rows = Rows,
                Columns = Columns,
            };
        }

        public override QuestionBase Deserialize(SerializableQuestion serializable) {
            QuestionGridBase deserializedQuestion = serializable.QuestionType switch {
                QuestionType.MultipleChoiceGrid => new QuestionMultipleChoiceGrid(serializable.Id),
                QuestionType.CheckboxGrid => new QuestionCheckboxGrid(serializable.Id),
            };

            if(serializable is SerializableGridQuestion gridSerializable) {
                deserializedQuestion.Title = gridSerializable.Title;
                deserializedQuestion.Description = gridSerializable.Description;
                deserializedQuestion.ViewPointId = gridSerializable.ViewPointId;
                deserializedQuestion.Rows = gridSerializable.Rows;
                deserializedQuestion.Columns = gridSerializable.Columns;

                string jsonString = JsonUtility.ToJson(gridSerializable);
            }

            return deserializedQuestion;
        }

    }

    [Serializable]
    public class SerializableGridQuestion : SerializableQuestion {
        public List<string> Rows = new();
        public List<string> Columns = new();
    }


}