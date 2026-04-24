using SurveySystem;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;
public class SurveyQuestionUIEditorGrid : SurveyQuestionUIEditor {

    #region Events

    public event Action<int> OnAddRow;
    public event Action<int> OnAddColumn;

    #endregion

    public SurveyQuestionUIEditorGrid(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder,
        bool isDeserialized = false) 
        : base(root, questionId, questionType, viewPoints, uiBuilder){
        
    }


    public void AddInitialAnswer() {

    }

    protected override void RegisterButtons() {
        throw new NotImplementedException();
    }

    protected override SurveyAnswerUIEditorString CreateAnswerUI(VisualElement element, int index, bool isOther) {
        throw new NotImplementedException();
    }

}
