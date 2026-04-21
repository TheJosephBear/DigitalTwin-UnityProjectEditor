namespace SurveySystem {
    public class QuestionCheckboxGrid : QuestionBase {
        // Multiple collumns, each collumn has its own answers i guess..? Collumn has a description text, each row also has a description text
        // Multiple answers
        public QuestionCheckboxGrid(int ID) : base(ID, QuestionType.CheckboxGrid) {
            MultipleAnswersAllowed = false;
        }

    }
}