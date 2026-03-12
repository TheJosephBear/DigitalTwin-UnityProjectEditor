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
        }

        public void RemoveQuestion(int idx) {
            if (idx >= 0 && idx < _questions.Count)
                _questions.RemoveAt(idx);
        }

        public void SetActiveQuestion(int idx) {
            if (idx >= 0 && idx < _questions.Count)
                ActiveQuestion = _questions[idx];
        }

        public QuestionBase GetQuestionById(int id) {
            return _questions.Find(a => a.Id == id);
        }

        public SerializableSurvey Serialize() {
            List<SerializableQuestion> questionsSerialized = new List<SerializableQuestion>();
            foreach (QuestionBase question in _questions) {
                questionsSerialized.Add(question.Serialize());
            }

            return new SerializableSurvey {
                Name = Name,
                Questions = questionsSerialized
            };
        }
    }

    [Serializable]
    public class SerializableSurvey {
        public string Name;
        public List<SerializableQuestion> Questions;
    }
}