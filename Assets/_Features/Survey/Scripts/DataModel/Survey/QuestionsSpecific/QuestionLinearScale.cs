namespace SurveySystem {
    public class QuestionLinearScale : QuestionBase {
        public int Min = 1;
        public int Max = 5;

        public QuestionLinearScale(int ID)
            : base(ID, QuestionType.LinearScale) {
            MultipleAnswersAllowed = false;
        }

    }
}