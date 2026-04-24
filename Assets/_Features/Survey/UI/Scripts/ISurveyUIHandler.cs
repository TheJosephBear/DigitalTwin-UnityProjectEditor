using SurveySystem;

public interface ISurveyUIHandler {
    void HandleQuestionTitleChanged(int questionId, string newText);
    void HandleQuestionDescriptionChanged(int questionId, string newText);
    void HandleQuestionViewPointSelected(int questionID, string viewPointID);
    void HandleAnswerAdded(int questionId);
    void HandleAnswerOtherAdded(int questionId);
    int GetQuestionIndex(SurveyQuestionUIEditor questionUI);
    void HandleQuestionDeleted(int questionIndex);
    void HandleQuestionMoved(int questionIndex, int direction);
    void HandleAnswerRemoved(AnswerBase answer);
    void HandleAnswerTextChanged(int questionId, int answerId, string newText);
}
