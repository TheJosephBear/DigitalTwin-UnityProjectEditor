using UnityEngine;

namespace SurveySystem {
    public class QuestionParagraph : QuestionBase {
        public QuestionParagraph(int ID) : base(ID, QuestionType.Paragraph) {
            MultipleAnswersAllowed = false;
            AddNewAnswer(true); // always one "other-like"
        }

        // Text question has exactly one free-text answer, the editor still asks for an initial one
        public override AnswerBase AddNewAnswer() {
            return AddNewAnswer(true);
        }

        public override AnswerBase AddNewAnswer(bool isOther) {
            if (_answers.Count > 0) return _answers[0];

            AnswerBase answer = new AnswerString {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = isOther
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public override QuestionBase Deserialize(SerializableQuestion serializable) {
            // The constructor answer would be added on top of the saved one
            _answers.Clear();
            base.Deserialize(serializable);

            // Older saves have the answer duplicated (one more on every load), keep just one
            if (_answers.Count > 1) _answers.RemoveRange(1, _answers.Count - 1);
            if (_answers.Count == 0) AddNewAnswer(true);
            ActiveAnswer = _answers[0];

            return this;
        }
    }
}
