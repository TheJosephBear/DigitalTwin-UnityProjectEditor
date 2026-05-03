using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SurveySystem {
    public class QuestionImageChoice : QuestionBase {

        public QuestionImageChoice(int ID) : base(ID, QuestionType.ImageChoice) {
            MultipleAnswersAllowed = false;
        }

        public override AnswerBase AddNewAnswer() {
            AnswerBase answer = new AnswerString {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = false
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public override AnswerBase AddNewAnswer(bool isOther) {
            AnswerBase answer = new AnswerString {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = isOther
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }
    }
}