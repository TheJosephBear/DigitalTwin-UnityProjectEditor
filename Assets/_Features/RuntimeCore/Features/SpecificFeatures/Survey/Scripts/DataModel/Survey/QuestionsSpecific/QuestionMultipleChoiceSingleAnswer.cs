using UnityEngine;

namespace SurveySystem {
    public class QuestionMultipleChoiceSingleAnswer : QuestionBase {
        public QuestionMultipleChoiceSingleAnswer(int ID) : base(ID, QuestionType.MultipleChoiceSingle) {
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
