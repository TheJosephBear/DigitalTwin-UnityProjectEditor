using UnityEngine;

namespace SurveySystem {
    public class QuestionParagraph : QuestionBase {
        public QuestionParagraph(int ID) : base(ID, QuestionType.Paragraph) {
            MultipleAnswersAllowed = false;
            AddNewAnswer(true); // always one "other-like"
        }

  //      public override bool HasPredefinedAnswers() => false;
   //     public override bool IsTextInputOnly() => true;

        public override AnswerBase AddNewAnswer() {
            if (_answers.Count == 0)
                return base.AddNewAnswer(true);

            return _answers[0]; // enforce single answer
        }
        
    }
}
