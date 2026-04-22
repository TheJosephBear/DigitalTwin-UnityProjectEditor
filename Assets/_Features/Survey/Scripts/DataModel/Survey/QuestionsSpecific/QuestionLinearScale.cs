namespace SurveySystem {
    public class QuestionLinearScale : QuestionBase {
        public int Min = 1;
        public int Max = 5;

        public QuestionLinearScale(int ID)
            : base(ID, QuestionType.LinearScale) {
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