using System.Collections.Generic;
using System;
using UnityEngine;

namespace SurveySystem {
    public class Survey {
        public string Name { get; set; }

        private List<QuestionBase> _questions = new();
        public QuestionBase ActiveQuestion { get; private set; }

        public IReadOnlyList<QuestionBase> Questions => _questions;

        public QuestionBase AddNewQuestion(QuestionBase question) {
            _questions.Add(question);
            ActiveQuestion = question;
            return question;
        }

        public void RemoveQuestion(int idx) {
            if (idx >= 0 && idx < _questions.Count)
                _questions.RemoveAt(idx);
        }

        public void SetActiveQuestion(int idx) {
            if (idx >= 0 && idx < _questions.Count)
                ActiveQuestion = _questions[idx];
        }

        public QuestionBase GetActiveQuestion() => ActiveQuestion;

    }
}