using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SurveyAnswerUIBase {

    protected bool _isOther = false;
    protected int _answerIndex;
    public int AnswerIndex => _answerIndex;

    protected VisualElement _answerElement;
    public VisualElement AnswerElement => _answerElement;
    protected SurveyQuestionUIBase _questionUIRef;



    public SurveyAnswerUIBase(VisualElement answerElement, int answerIndex, SurveyQuestionUIBase questionUI, bool isOther) {
        _answerElement = answerElement;
        _answerIndex = answerIndex;
        _questionUIRef = questionUI;
        _isOther = isOther;

        if (_isOther) {
            _answerElement.Q<TextField>().isReadOnly = true;
        }

        RegisterAnswerEvents(); 
    }

    protected abstract void RegisterAnswerEvents();


    public void UpdateIndex(int newIndex) {
        _answerIndex = newIndex;
    }
    public virtual void HideCurrentModal() { } // Shouldnt be here but it fixed a big issue the easiest way

}
