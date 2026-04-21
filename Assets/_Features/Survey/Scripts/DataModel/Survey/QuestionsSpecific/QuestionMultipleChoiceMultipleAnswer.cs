using UnityEngine;

namespace SurveySystem {
    public class QuestionMultipleChoiceMultipleAnswer : QuestionBase {
        public QuestionMultipleChoiceMultipleAnswer(int ID)
            : base(ID, QuestionType.MultipleChoiceMultiple) {
            MultipleAnswersAllowed = true;
        }

     //   public override bool AllowsMultipleAnswers() => true;
    }
}
