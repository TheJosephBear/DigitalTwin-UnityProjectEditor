using UnityEngine;

namespace SurveySystem {
    public class QuestionMultipleChoiceGrid : QuestionBase {
        // Multiple collumns, each collumn has its own answers i guess..? Collumn has a description text, each row also has a description text
        public QuestionMultipleChoiceGrid(int ID) : base(ID, QuestionType.MultipleChoiceGrid) {
        }

    }
}