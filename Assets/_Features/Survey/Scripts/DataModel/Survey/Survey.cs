using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace SurveySystem {
    public class Survey {
        public string Name { get; set; }

        private List<QuestionBase> _questions = new();
        public QuestionBase ActiveQuestion { get; private set; }

        public IReadOnlyList<QuestionBase> Questions => _questions;

        public void AddNewQuestion(QuestionBase question) {
            _questions.Add(question);
            ActiveQuestion = question;
        //    question.AddNewAnswer();
        }

        public void RemoveQuestion(int idx) {
            if (idx >= 0 && idx < _questions.Count)
                _questions.RemoveAt(idx);
        }

        public void InsertQuestion(int index, QuestionBase question) {
            if (index < 0 || index >= _questions.Count) {
                _questions.Add(question);
            } else {
                _questions.Insert(index, question);
            }
            ActiveQuestion = question;
        }

        public void MoveQuestion(int index, int direction) {
            Debug.Log($"(Builder) moving question index {index} in direction {direction}");
            int targetIndex = index + direction;
            if (targetIndex < 0 || targetIndex >= _questions.Count) return;

            var question = _questions[index];
            _questions.RemoveAt(index);
            _questions.Insert(targetIndex, question);
            Debug.Log($"(Builder) Swapped");
        }

        public void MoveAnswer(int questionIndex, int answerIndex, int direction) {
            // Validate question index
            if (questionIndex < 0 || questionIndex >= _questions.Count) return;

            // Delegate the movement to the specific question
            _questions[questionIndex].MoveAnswer(answerIndex, direction);
        }

        public void SetActiveQuestion(int idx) {
            if (idx >= 0 && idx < _questions.Count)
                ActiveQuestion = _questions[idx];
        }

        public QuestionBase GetQuestionById(int id) {
            return _questions.Find(a => a.Id == id);
        }

        public List<QuestionBase> GetAllQuestions() {
            return Questions.ToList();
        }

        #region Serialization

        public SurveySerializable Serialize() {
            List<SerializableQuestion> questionsSerialized = new List<SerializableQuestion>();
            foreach (QuestionBase question in _questions) {
                questionsSerialized.Add(question.Serialize());
            }

            return new SurveySerializable {
                Name = Name,
                Questions = questionsSerialized
            };
        }

        public void Deserialize(SurveySerializable serializable) {
            Name = serializable.Name;
            foreach (SerializableQuestion q in serializable.Questions) {
                _questions.Add(QuestionBase.CreateAndDeserialize(q));
            }
        }

        #endregion
    }

    [Serializable]
    public class SurveySerializable {
        public string Name;
        [SerializeReference]
        public List<SerializableQuestion> Questions;
    }
}