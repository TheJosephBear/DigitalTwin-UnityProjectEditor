using SurveySystem;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using UnityEngine;

namespace SurveySystem {
    public class SurveyResponseManager {
        private Survey _activeSurvey;
        private SurveySubmission _currentSubmission;

        public void Initialize(Survey survey) {
            Debug.Log("Initialized with this many questions: "+ survey.Questions.Count);
            _activeSurvey = survey;
            _currentSubmission = new SurveySubmission {
                SurveyName = survey.Name,
                Timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Pre-fill the responses list based on the survey structure
            foreach (var question in _activeSurvey.Questions) {
                _currentSubmission.Responses.Add(new QuestionResponse {
                    QuestionId = question.Id,
                    Type = question.QuestionType
                });
            }
        }

        /// <summary>
        /// Registers a selection for Single Choice or Linear Scale
        /// </summary>
        public void RegisterAnswer(int questionID, int answerID, string otherText = null) {
            var response = _currentSubmission.Responses.Find(r => r.QuestionId == questionID);
            if (response == null) return;

            response.SelectedIdx = answerID;
            response.ResponseText = otherText;
        }

        /// <summary>
        /// Registers a selection for Multiple Choice
        /// </summary>
        public void RegisterMultipleAnswers(int questionID, List<int> answerIndices) {
            var response = _currentSubmission.Responses.Find(r => r.QuestionId == questionID);
            if (response == null) return;

            response.SelectedIndices = answerIndices;
        }

        /// <summary>
        /// Registers text for Open Ended questions
        /// </summary>
        public void RegisterTextAnswer(int questionID, string text) {
            var response = _currentSubmission.Responses.Find(r => r.QuestionId == questionID);
            if (response == null) return;

            response.ResponseText = text;
        }

        public void RegisterGridAnswer(int questionId, int rowIdx, int columnIdx, bool value = true) {
            var response = _currentSubmission.Responses.Find(r => r.QuestionId == questionId);
            if (response == null) return;

            // Find the response entry for this specific row
            var rowResponse = response.GridResponses.Find(gr => gr.RowIdx == rowIdx);
            if (rowResponse == null) {
                rowResponse = new GridRowResponse { RowIdx = rowIdx };
                response.GridResponses.Add(rowResponse);
            }

            if (response.Type == QuestionType.MultipleChoiceGrid) {
                // Radio button logic: only one column index per row
                rowResponse.SelectedColumnIdx = columnIdx;
            } else if (response.Type == QuestionType.CheckboxGrid) {
                // Checkbox logic: list of toggled columns
                if (rowResponse.SelectedColumnIndices == null)
                    rowResponse.SelectedColumnIndices = new List<int>();

                if (value) {
                    if (!rowResponse.SelectedColumnIndices.Contains(columnIdx))
                        rowResponse.SelectedColumnIndices.Add(columnIdx);
                } else {
                    rowResponse.SelectedColumnIndices.Remove(columnIdx);
                }
            }
        }

        public string ExportResponseJson() {
            return JsonUtility.ToJson(_currentSubmission, true);
        }
    }

    [Serializable]
    public class QuestionResponse {
        public int QuestionId;
        public QuestionType Type;
        public int SelectedIdx = -1;             // For SingleChoice/Range
        public List<int> SelectedIndices = null; // For MultipleChoice
        public string ResponseText = null;       // For OpenEnded or "Other" text
        public List<GridRowResponse> GridResponses = new();
    }

    [Serializable]
    public class SurveySubmission {
        public string SurveyName;
        public string Timestamp;
        public List<QuestionResponse> Responses = new();
    }

    [Serializable]
    public class GridRowResponse {
        public int RowIdx;
        public int SelectedColumnIdx = -1;       // For MultipleChoiceGrid
        public List<int> SelectedColumnIndices;  // For CheckboxGrid
    }
}