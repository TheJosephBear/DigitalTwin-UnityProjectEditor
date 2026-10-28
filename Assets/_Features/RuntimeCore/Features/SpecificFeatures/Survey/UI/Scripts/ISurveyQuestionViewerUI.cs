using System;

public interface ISurveyQuestionViewerUI : ISurveyQuestionUI {
    event Action<int, int> OnAnswerSelected;
    event Action<int, int, string> OnAnswerTextFilled;
}