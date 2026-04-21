using QuestionnaireToolkit.Scripts.SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurveySystem {
    public class QuestionBase {
        public int Id { get; protected set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ViewPointId { get; protected set; }
        public QuestionType QuestionType { get; protected set; }
        public bool MultipleAnswersAllowed { get; protected set; } // Allow selecting multiple answers
        protected List<AnswerBase> _answers = new();
        public AnswerBase ActiveAnswer { get; protected set; }
        public IReadOnlyList<AnswerBase> Answers => _answers;

        public QuestionBase(int ID) {
            Id = ID;
        }

        public QuestionBase(int ID, QuestionType type) {
            Id = ID;
            QuestionType = type;
        }

        public virtual AnswerBase AddNewAnswer() {
            AnswerBase answer = new AnswerBase {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = false
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public virtual AnswerBase AddNewAnswer(bool isOther) {
            AnswerBase answer = new AnswerBase {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = isOther
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public virtual void AddExistingAnswer(AnswerBase answer) {
            _answers.Add(answer);
            ActiveAnswer = answer;
        }

        public void RemoveAnswer(int idx) {
            _answers.RemoveAll(a => a.Idx == idx);
        }

        public void SetActiveAnswer(int idx) {
            ActiveAnswer = _answers.Find(a => a.Idx == idx);
        }

        public AnswerBase GetAnswerByIdx(int idx) {
            return _answers.Find(a => a.Idx == idx);
        }

        public void SetViewPointID(string vpID) {
            ViewPointId = vpID;
        }

        public SerializableQuestion Serialize() {
            return new SerializableQuestion {
                Id = Id,
                Title = Title,
                Description = Description,
                ViewPointId = ViewPointId,
                QuestionType = QuestionType,
                Answers = _answers
            };
        }

        public QuestionBase Deserialize(SerializableQuestion serializable) {
            QuestionBase deserializedQuestion = serializable.QuestionType switch {
                QuestionType.MultipleChoiceSingle => new QuestionMultipleChoiceSingleAnswer(serializable.Id),
                QuestionType.MultipleChoiceMultiple => new QuestionMultipleChoiceMultipleAnswer(serializable.Id),
                QuestionType.ShortAnswer => new QuestionParagraph(serializable.Id),
                QuestionType.Paragraph => new QuestionParagraph(serializable.Id),
                QuestionType.Dropdown => new QuestionMultipleChoiceSingleAnswer(serializable.Id),
                QuestionType.MultipleChoiceGrid => new QuestionMultipleChoiceGrid(serializable.Id),
                QuestionType.CheckboxGrid => new QuestionCheckboxGrid(serializable.Id),
                QuestionType.ImageChoice => new QuestionMultipleChoiceSingleAnswer(serializable.Id),
                QuestionType.LinearScale => new QuestionLinearScale(serializable.Id),
            };
            
           deserializedQuestion.Title = serializable.Title;
           deserializedQuestion.Description = serializable.Description;
           deserializedQuestion.ViewPointId = serializable.ViewPointId;
           foreach(AnswerBase answer in serializable.Answers){ 
             deserializedQuestion.AddExistingAnswer(answer);
           }

            return deserializedQuestion;
        }
    }

    [Serializable]
    public class SerializableQuestion{
        public int Id;
        public string Title;
        public string Description;
        public string ViewPointId;
        public QuestionType QuestionType;
        public List<AnswerBase> Answers;
    }

    public enum QuestionType {
        MultipleChoiceSingle,
        MultipleChoiceMultiple,
        ShortAnswer,
        Paragraph,
        Dropdown,
        MultipleChoiceGrid,
        CheckboxGrid,
        ImageChoice,
        LinearScale,
    }
}