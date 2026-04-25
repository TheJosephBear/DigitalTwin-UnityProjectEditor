using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SurveyAnswerUIViewer : SurveyAnswerUIBase {

    public event Action<int, string> OnAnswerSelected;

    public SurveyAnswerUIViewer(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditorString questionUI, bool isOther) 
        : base(answerElement, answerIndex, questionUI, isOther) {

    }

}
