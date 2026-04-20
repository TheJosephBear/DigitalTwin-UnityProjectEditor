using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace SurveySystem {
    /// <summary>
    /// Manages survey creation. Provides interface for creation and editing of a survey data model.
    /// </summary>
    public class SurveyBuilder {
        private Survey _activeSurvey;
        private int _nextId = 0;

        public Survey CreateNewSurvey() {
            _activeSurvey = new Survey();
            return _activeSurvey;
        }

        public bool HasActiveSurvey() {
            return _activeSurvey != null;   
        }

        public void SetActiveSurvey(Survey survey) {
            _activeSurvey = survey;
        }

        public void SetSurveyName(string name) {
            _activeSurvey.Name = name;
        }

        public QuestionBase AddNewQuestion(QuestionType type) {
            QuestionBase question = type switch {
                QuestionType.MultipleChoiceSingle => new QuestionMultipleChoiceSingleAnswer(_nextId++),
                //       QuestionType.MultipleChoiceMultiple => new MultiChoiceMultiple(),
                _ => new QuestionBase(_nextId++, type)
            };

            _activeSurvey.AddNewQuestion(question);
            return question;
        }

        public void RemoveQuestion(int idx) {
            _activeSurvey.RemoveQuestion(idx);
        }

        public void SetQuestionTitle(string title) {
            _activeSurvey.ActiveQuestion.Title = title;
        }

        public void SetQuestionTitle(QuestionBase question, string text) {
            question.Title = text;
        }

        public void SetQuestionTitle(int questionId, string text) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.Title = text;
        }

        public void SetQuestionDescription(string description) {
            _activeSurvey.ActiveQuestion.Description = description;
        }

        public void SetQuestionDescription(QuestionBase question, string text) {
            question.Description = text;
        }

        public void SetQuestionDescription(int questionId, string text) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.Description = text;
            ExportSurveyAsJson();
        }

        public void SetQuestionViewPoint(int questionId, string viewPointID) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.SetViewPointID(viewPointID);
        }

        public void AddNewAnswerToQuestion() {
            _activeSurvey.ActiveQuestion.AddNewAnswer();
        }
        public void AddNewAnswerToQuestion(QuestionBase question) {
            question.AddNewAnswer();
        }

        public void AddNewAnswerToQuestion(int questionId) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.AddNewAnswer();
        }

        public void AddNewAnswerToQuestion(int questionId, bool isOther) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.AddNewAnswer(isOther);
        }

        public void SetActiveAnswer(int idx) {
            _activeSurvey.ActiveQuestion.SetActiveAnswer(idx);
        }

        public void SetAnswerText(string text) {
            SetAnswerText(_activeSurvey.ActiveQuestion.ActiveAnswer, text);
        }

        public void SetAnswerText(AnswerBase answer, string text) {
            if (answer == null) return;

            // Bezpečné přetypování - zkusíme, jestli odpověď podporuje text
            if (answer is AnswerChoice choice) {
                choice.Text = text;
            } else if (answer is AnswerString strAnswer) {
                strAnswer.Text = text;
            }
        }

        public void SetAnswerText(int questionId, int answerId, string text) {
            var answer = _activeSurvey.GetQuestionById(questionId)?.GetAnswerByIdx(answerId);
            SetAnswerText(answer, text);
        }

        public void RemoveAnswer(int idx) {
            _activeSurvey.ActiveQuestion.RemoveAnswer(idx);
        }

        public Survey GetActiveSurvey() {
            return _activeSurvey;
        }

        #region Serialization

        public string ExportSurveyAsJson() {
            string jsonString = JsonUtility.ToJson(_activeSurvey.Serialize());

            return jsonString;
        }

        public void DeserializeFromJson(string json) {
            Debug.Log("Desserializing survey");
            CreateNewSurvey();
            _activeSurvey.Deserialize(JsonUtility.FromJson<SurveySerializable>(json));
        }

        #endregion

    }
}