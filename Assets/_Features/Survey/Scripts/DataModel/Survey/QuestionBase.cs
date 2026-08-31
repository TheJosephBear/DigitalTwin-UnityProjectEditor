using QuestionnaireToolkit.Scripts.SimpleJSON;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurveySystem {
    public class QuestionBase {
        public int Id { get; protected set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string ViewPointId { get; protected set; }
        public string ImageID { get; protected set; }
        public QuestionType QuestionType { get; protected set; }
        public bool MultipleAnswersAllowed { get; protected set; } // Allow selecting multiple answers
        public bool IsGrid { get; protected set; }
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

        public virtual void MoveAnswer(int index, int direction) {
            int targetIndex = index + direction;
            if (targetIndex < 0 || targetIndex >= _answers.Count) return;

            var answer = _answers[index];
            _answers.RemoveAt(index);
            _answers.Insert(targetIndex, answer);

            // Update Idx properties if you rely on them for persistence
            for (int i = 0; i < _answers.Count; i++) {
                _answers[i].Idx = i;
            }
        }

        public void RemoveAnswer(int idx) {
            _answers.RemoveAt(idx);
            for (int i = 0; i < _answers.Count; i++) {
                _answers[i].Idx = i;
            }
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

        public void SetImageID(string imageId) {
            ImageID = imageId;
        }

        public virtual SerializableQuestion Serialize() {
            return new SerializableQuestion {
                Id = Id,
                Title = Title,
                Description = Description,
                IsRequired = IsRequired,
                ViewPointId = ViewPointId,
                ViewpointID = ViewPointId,
                ImageId = ImageID,
                ImageID = ImageID,
                QuestionType = QuestionType,
                Answers = _answers
            };
        }

        public static QuestionBase CreateAndDeserialize(SerializableQuestion serializable) {
            QuestionBase deserializedQuestion = serializable.QuestionType switch {
                QuestionType.MultipleChoiceSingle => new QuestionMultipleChoiceSingleAnswer(serializable.Id),
                QuestionType.MultipleChoiceMultiple => new QuestionMultipleChoiceMultipleAnswer(serializable.Id),
                QuestionType.ShortAnswer => new QuestionParagraph(serializable.Id),
                QuestionType.Paragraph => new QuestionParagraph(serializable.Id),
                QuestionType.Dropdown => new QuestionMultipleChoiceSingleAnswer(serializable.Id),
                QuestionType.MultipleChoiceGrid => new QuestionMultipleChoiceGrid(serializable.Id),
                QuestionType.CheckboxGrid => new QuestionCheckboxGrid(serializable.Id),
                QuestionType.ImageChoice => new QuestionImageChoice(serializable.Id),
                QuestionType.LinearScale => new QuestionLinearScale(serializable.Id),
            };

            return deserializedQuestion.Deserialize(serializable);
        }

        public virtual QuestionBase Deserialize(SerializableQuestion serializable) {
            Title = serializable.Title;
            Description = serializable.Description;
            IsRequired = serializable.IsRequired;
            ViewPointId = !string.IsNullOrEmpty(serializable.ViewPointId) ? serializable.ViewPointId : (!string.IsNullOrEmpty(serializable.ViewpointID) ? serializable.ViewpointID : "");
            ImageID = !string.IsNullOrEmpty(serializable.ImageId) ? serializable.ImageId : (!string.IsNullOrEmpty(serializable.ImageID) ? serializable.ImageID : "");
            foreach (AnswerBase answer in serializable.Answers) {
                AddExistingAnswer(answer);
            }

            return this;
        }
    }

    [Serializable]
    public class SerializableQuestion {
        public int Id;
        public string Title;
        public string Description;
        public bool IsRequired;
        public string ViewPointId;
        public string ViewpointID;
        public string ImageId;
        public string ImageID;
        public QuestionType QuestionType; 
        [SerializeReference]
        public List<AnswerBase> Answers;
        public List<string> Rows = new();
        public List<string> Columns = new();
        public int Min;
        public int Max;
        public string ScaleType;
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