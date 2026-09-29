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

        public void RegisterAnswer(int questionID, int answerID, bool isSelected = true, string textValue = null) {
            var response = _currentSubmission.Responses.Find(r => r.QuestionId == questionID);
            if (response == null) return;

            // 1. Handle Multiple Choice (Multiple Answers)
            if (response.Type == QuestionType.MultipleChoiceMultiple) {
                if (response.SelectedIndices == null) response.SelectedIndices = new List<int>();

                if (isSelected) {
                    if (!response.SelectedIndices.Contains(answerID))
                        response.SelectedIndices.Add(answerID);
                } else {
                    response.SelectedIndices.Remove(answerID);
                }
            }
            // 2. Handle Text-Only Questions (Paragraph / Short Answer)
            else if (response.Type == QuestionType.Paragraph || response.Type == QuestionType.ShortAnswer) {
                response.ResponseText = textValue;
                // Mark as "selected" (0) simply to indicate it has a response, 
                // though ResponseText is the primary data here.
                response.SelectedIdx = string.IsNullOrEmpty(textValue) ? -1 : 0;
            }
            // 3. Handle Single Choice / Linear Scale / Dropdown
            else {
                response.SelectedIdx = isSelected ? answerID : -1;
            }

            // 4. Handle "Other" text for Choice questions
            // This allows a choice question to have both a SelectedIdx AND custom text
            if (textValue != null && (response.Type != QuestionType.Paragraph && response.Type != QuestionType.ShortAnswer)) {
                response.ResponseText = textValue;
            }
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

        public void RegisterScaleAnswer(int questionId, int rowIdx, int value) {
            var response = _currentSubmission.Responses.Find(r => r.QuestionId == questionId);
            if (response == null) return;

            var scaleResponse = response.ScaleResponses.Find(sr => sr.RowIdx == rowIdx);
            if (scaleResponse == null) {
                scaleResponse = new ScaleRowResponse { RowIdx = rowIdx, Value = value };
                response.ScaleResponses.Add(scaleResponse);
            } else {
                scaleResponse.Value = value;
            }

            // Also keep SelectedIdx updated for single row cases
            response.SelectedIdx = value;
        }

        public QuestionValidationResult ValidateQuestion(QuestionBase question) {
            var result = new QuestionValidationResult();
            if (question == null) return result;

            var response = _currentSubmission?.Responses.Find(r => r.QuestionId == question.Id);

            switch (question.QuestionType) {
                case QuestionType.MultipleChoiceSingle:
                case QuestionType.Dropdown: {
                    int otherIdx = FindOtherAnswerIdx(question);
                    int selected = response?.SelectedIdx ?? -1;
                    result.IsAnswered = selected >= 0;
                    if (result.IsAnswered && otherIdx >= 0 && selected == otherIdx && string.IsNullOrWhiteSpace(response.ResponseText)) {
                        result.IsAnswered = false;
                        result.MissingOtherText = true;
                    }
                    break;
                }
                case QuestionType.MultipleChoiceMultiple: {
                    int otherIdx = FindOtherAnswerIdx(question);
                    var selected = response?.SelectedIndices;
                    result.IsAnswered = selected != null && selected.Count > 0;
                    if (result.IsAnswered && otherIdx >= 0 && selected.Contains(otherIdx) && string.IsNullOrWhiteSpace(response.ResponseText)) {
                        result.IsAnswered = false;
                        result.MissingOtherText = true;
                    }
                    break;
                }
                case QuestionType.ImageChoice:
                    result.IsAnswered = response != null && response.SelectedIdx >= 0;
                    break;
                case QuestionType.Paragraph:
                case QuestionType.ShortAnswer:
                    result.IsAnswered = response != null && !string.IsNullOrWhiteSpace(response.ResponseText);
                    break;
                case QuestionType.MultipleChoiceGrid:
                case QuestionType.CheckboxGrid: {
                    int rowCount = question is QuestionGridBase grid ? grid.GetRowCount() : 0;
                    for (int row = 0; row < rowCount; row++) {
                        var rowResponse = response?.GridResponses.Find(gr => gr.RowIdx == row);
                        bool rowAnswered = question.QuestionType == QuestionType.MultipleChoiceGrid
                            ? rowResponse != null && rowResponse.SelectedColumnIdx >= 0
                            : rowResponse?.SelectedColumnIndices != null && rowResponse.SelectedColumnIndices.Count > 0;
                        if (!rowAnswered) result.MissingRows.Add(row);
                    }
                    result.IsAnswered = result.MissingRows.Count == 0;
                    break;
                }
                case QuestionType.LinearScale: {
                    for (int row = 0; row < question.Answers.Count; row++) {
                        bool rowAnswered = response != null && response.ScaleResponses.Exists(sr => sr.RowIdx == row);
                        if (!rowAnswered) result.MissingRows.Add(row);
                    }
                    result.IsAnswered = result.MissingRows.Count == 0;
                    break;
                }
                default:
                    result.IsAnswered = true;
                    break;
            }

            return result;
        }

        private static int FindOtherAnswerIdx(QuestionBase question) {
            for (int i = 0; i < question.Answers.Count; i++) {
                if (question.Answers[i].IsOther) return i;
            }
            return -1;
        }

        public string ExportResponseJson() {
            return JsonUtility.ToJson(_currentSubmission, true);
        }
    }

    public class QuestionValidationResult {
        public bool IsAnswered;
        public List<int> MissingRows = new();   // Grid / LinearScale rows without answer
        public bool MissingOtherText;           // "Other" option selected but its text is empty
    }

    [Serializable]
    public class QuestionResponse {
        public int QuestionId;
        public QuestionType Type;
        public int SelectedIdx = -1;             // For SingleChoice/Range
        public List<int> SelectedIndices = null; // For MultipleChoice
        public string ResponseText = null;       // For OpenEnded or "Other" text
        public List<GridRowResponse> GridResponses = new();
        public List<ScaleRowResponse> ScaleResponses = new();
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

    [Serializable]
    public class ScaleRowResponse {
        public int RowIdx;
        public int Value;
    }
}