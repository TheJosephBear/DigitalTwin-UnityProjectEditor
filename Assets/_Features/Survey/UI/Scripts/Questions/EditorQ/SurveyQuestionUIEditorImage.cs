using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIEditorImage : SurveyQuestionUIEditor {

    public event Action<int, int, string> OnAnswerImageChanged;
    public event Action<int> OnAnswerAdded;
    public event Action<int> OnAnswerRemoved; // Nejde na to tlaèítko kliknou tak zatím nemusíme implementit

    public SurveyQuestionUIEditorImage(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        var answerUI = new SurveyAnswerUIEditorImage(element, index, this, isOther);

        // Relay the event up to the Builder/Manager
        answerUI.OnAnswerImageChanged += (idx, imageId) => {
            OnAnswerImageChanged?.Invoke(QuestionID, idx, imageId);
        };

        return answerUI;
    }

    protected override void RegisterButtons() {
        // Find the "add option" button (ensure this ID matches your UXML)
        var addOptionButton = _root.Q<Button>("add-option-button");
        if (addOptionButton != null) {
            addOptionButton.clicked += () => {
                var newAnswer = AddAnswerUI();
                OnAnswerAdded?.Invoke(QuestionID);
            };
        }

        // Standard question buttons (Move, Delete, Upload main question image)
        RegisterQuestionModalButtonEvents();
    }

    public void SetAnswerImage(int answerIndex, string imageId) {
        if (answerIndex >= 0 && answerIndex < _addedAnswers.Count) {
            if (_addedAnswers[answerIndex] is SurveyAnswerUIEditorImage imgAnswer) {
                imgAnswer.SetImage(imageId);
            }
        }
    }

    public void AddAnswerWithImage(string imageId) {
        // 1. Create the UI element (inherited from SurveyQuestionUIBase)
        SurveyAnswerUIBase newAnswerUI = AddAnswerUI();

        // 2. Cast and set the image
        if (newAnswerUI is SurveyAnswerUIEditorImage imageAnswer) {
            imageAnswer.SetImage(imageId);

            // 3. Notify the builder/manager so the data model is updated
         //   OnAnswerAdded?.Invoke(QuestionID);
        //    OnAnswerImageChanged?.Invoke(QuestionID, imageAnswer.AnswerIndex, imageId);
        }
    }
}