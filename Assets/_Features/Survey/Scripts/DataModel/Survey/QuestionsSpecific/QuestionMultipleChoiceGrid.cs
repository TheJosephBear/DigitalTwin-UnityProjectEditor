using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace SurveySystem {
    public class QuestionMultipleChoiceGrid : QuestionBase {
        // Multiple collumns, each collumn has its own answers i guess..? Collumn has a description text, each row also has a description text

        List<string> Rows = new List<string>();
        List<string> Collumns = new List<string>();

        public QuestionMultipleChoiceGrid(int ID) : base(ID, QuestionType.MultipleChoiceGrid) {
            MultipleAnswersAllowed = false;
            IsGrid = true;
        }

        public void AddRow(string rowText) {
            Rows.Add(rowText);
            UpdateAnswerList();
        }

        public void AddCollumn(string rowText) {
            Collumns.Add(rowText);
            UpdateAnswerList();
        }

        void UpdateAnswerList() {
            for (int i = 0; i < Rows.Count; i++) {
                for (int j = 0; j < Collumns.Count; j++) {
                    foreach (AnswerGrid answer in _answers) {
                        if (answer.Row == i && answer.Collumn == j) {
                            continue;
                        } else {
                            _answers.Add(new AnswerGrid(i, j));
                        }
                    }
                }
            }
        }

    }
}