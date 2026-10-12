using UnityEngine;

namespace SurveySystem {
    public class QuestionParagraph : QuestionBase {
        public QuestionParagraph(int ID) : base(ID, QuestionType.Paragraph) {
            MultipleAnswersAllowed = false;
            AddNewAnswer(true); // always one "other-like"
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
