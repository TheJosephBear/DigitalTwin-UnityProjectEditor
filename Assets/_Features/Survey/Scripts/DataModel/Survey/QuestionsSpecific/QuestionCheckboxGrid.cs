namespace SurveySystem {
    public class QuestionCheckboxGrid : QuestionGridBase {

        public QuestionCheckboxGrid(int id)
            : base(id, QuestionType.CheckboxGrid) {
            MultipleAnswersAllowed = true;
        }

        protected override AnswerGrid CreateAnswer(int row, int column) {
            return new AnswerGrid(row, column);
        }
    }
}