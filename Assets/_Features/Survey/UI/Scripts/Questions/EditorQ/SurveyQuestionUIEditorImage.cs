using SurveySystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyQuestionUIEditorImage : SurveyQuestionUIEditor {

    public SurveyQuestionUIEditorImage(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder) 
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

    }

    protected override SurveyAnswerUIBase CreateAnswerUI(VisualElement element, int index, bool isOther) {
        throw new System.NotImplementedException();
    }

    protected override void RegisterButtons() {
        throw new System.NotImplementedException();
    }
}
