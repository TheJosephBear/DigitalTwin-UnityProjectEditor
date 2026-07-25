using SurveySystem;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIViewerImage : SurveyQuestionUIViewer {

    public event Action<int, int, bool> OnAnswerSelected;

    public SurveyQuestionUIViewerImage(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {
    }

    // This is called by the UI Builder when iterating through the question's answers
    public override SurveyAnswerUIBase AddAnswer(string imageId, bool isOther = false) {
        if (_optionsList == null || _answerTemplate == null) return null;

        // 1. Instantiate the Template
        VisualElement answerElement = _answerTemplate.Instantiate();
        int answerIndex = _addedAnswers.Count;

        // 2. Initialize the Answer Logic
        SurveyAnswerUIViewerImage answerUI = new SurveyAnswerUIViewerImage(answerElement, answerIndex, this, isOther);

        // 3. Set the Image
        answerUI.SetImage(imageId);

        // 4. Bind Selection Event
        answerUI.OnSelected += (qId, aIdx, val) => {
            HandleSingleSelection(aIdx);
            OnAnswerSelected?.Invoke(qId, aIdx, val);
        };

        // 5. Add to UI Layout
        _optionsList.Add(answerElement);
        _addedAnswers.Add(answerUI);

        return answerUI;
    }

    private void HandleSingleSelection(int selectedIndex) {
        // If it's a single choice question, deselect all other UI elements
        for (int i = 0; i < _addedAnswers.Count; i++) {
            if (_addedAnswers[i] is SurveyAnswerUIViewerImage imgAnswer) {
                imgAnswer.SetSelected(i == selectedIndex);
            }
        }
    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        return null; // Not used in Viewer path (AddAnswer handles it)
    }

    protected override void RegisterButtons() {
        base.RegisterButtons();
    }

    protected override void RegisterTextInputs() {

    }

    protected override void RegisterDropdown() {

    }
}