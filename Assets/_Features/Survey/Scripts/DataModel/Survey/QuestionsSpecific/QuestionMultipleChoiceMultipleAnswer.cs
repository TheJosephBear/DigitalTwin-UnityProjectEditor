using UnityEngine;

namespace SurveySystem {
    public class QuestionMultipleChoiceMultipleAnswer : QuestionBase {
        public QuestionMultipleChoiceMultipleAnswer(int ID)
            : base(ID, QuestionType.MultipleChoiceMultiple) {
            MultipleAnswersAllowed = true;
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
