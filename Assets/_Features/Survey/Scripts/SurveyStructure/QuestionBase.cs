using System.Collections.Generic;
using UnityEngine;

namespace SurveySystem {
    public class QuestionBase {
        public int Id { get; protected set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestionType QuestionType { get; protected set; }

        protected List<AnswerBase> _answers = new();
        public AnswerBase ActiveAnswer { get; protected set; }

        public IReadOnlyList<AnswerBase> Answers => _answers;

        public QuestionBase(int ID) {
            Id = ID;
            AddNewAnswer();
        }

        public virtual AnswerBase AddNewAnswer() {
            AnswerBase answer = new AnswerBase {
                Idx = _answers.Count,
                Text = string.Empty
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public void RemoveAnswer(int idx) {
            _answers.RemoveAll(a => a.Idx == idx);
        }

        public void SetActiveAnswer(int idx) {
            ActiveAnswer = _answers.Find(a => a.Idx == idx);
        }

        public AnswerBase GetAnswerByIdx(int idx) {
            return _answers.Find(a => a.Idx == idx);
        }
    }

    public enum QuestionType {
        MultipleChoiceSingle,
        MultipleChoiceMultiple,
        LinearScale,
        OpenEnded,
    }
}