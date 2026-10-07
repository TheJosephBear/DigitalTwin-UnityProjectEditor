using UnityEngine.UIElements;

public interface ISurveyQuestionUI {
    public int QuestionID { get; }
    VisualElement QuestionElement { get; }

    public void SetTitle(string title);
    public void SetDescription(string desc);
    public void AddAnswer(string answerText, bool isOther);

}