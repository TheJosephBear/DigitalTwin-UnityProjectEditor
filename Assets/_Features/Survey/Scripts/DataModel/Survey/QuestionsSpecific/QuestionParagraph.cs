using UnityEngine;

namespace SurveySystem {
    public class QuestionParagraph : QuestionBase {
        public QuestionParagraph(int ID) : base(ID, QuestionType.Paragraph) {
            MultipleAnswersAllowed = false;
            AddNewAnswer(true); // always one "other-like"
        }
    }
}
