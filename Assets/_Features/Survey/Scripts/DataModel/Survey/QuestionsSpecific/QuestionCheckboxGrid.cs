namespace SurveySystem {
    public class QuestionCheckboxGrid : QuestionGridBase {

        public QuestionCheckboxGrid(int id)
            : base(id, QuestionType.MultipleChoiceGrid) {
            MultipleAnswersAllowed = false;
        }

        protected override AnswerGrid CreateAnswer(int row, int column) {
            return new AnswerGrid(row, column);
        }
    }
}