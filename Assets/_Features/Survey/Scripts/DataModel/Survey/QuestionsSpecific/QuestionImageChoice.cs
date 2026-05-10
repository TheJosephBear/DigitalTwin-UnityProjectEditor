using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SurveySystem {
    public class QuestionImageChoice : QuestionBase {

        public QuestionImageChoice(int ID) : base(ID, QuestionType.ImageChoice) {
            MultipleAnswersAllowed = false;
        }

        public void SetAnswerImageID(int answerID, string imageId) {
            if (_answers.Find(x => x.Idx == answerID) is AnswerImage imageAnswer) {
                imageAnswer.ImageID = imageId;
            }
        }

        public override AnswerBase AddNewAnswer() {
            AnswerBase answer = new AnswerImage {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = false
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public override AnswerBase AddNewAnswer(bool isOther) {
            // I dont think we do that here
            return AddNewAnswer();
        }

        public override QuestionBase Deserialize(SerializableQuestion serializable) {
            // Call base to handle Title, Description, etc.
            base.Deserialize(serializable);

            // Re-clear answers because the base might have added them as AnswerBase
            _answers.Clear();

            foreach (var ans in serializable.Answers) {
                // Check if the serialized data is an AnswerImage
                if (ans is AnswerImage imgAns) {
                    _answers.Add(imgAns);
                } else {
                    // Fallback: Convert base Answer to ImageAnswer if necessary
                    _answers.Add(new AnswerImage {
                        Idx = ans.Idx,
                        Text = ans.Text,
                        IsOther = ans.IsOther,
                        ImageID = "" // Will be empty until user uploads
                    });
                }
            }

            return this;
        }
    }
}