using System;

public interface ISurveyQuestionBuilderUI : ISurveyQuestionUI {
    public event Action<int, string> OnTitleChanged;
    public event Action<int, string> OnDescriptionChanged;
    public event Action<int> OnQuestionDeleted;
    public event Action<int, int> OnQuestionMoved;
    public event Action<int, SurveyAnswerUIEditorString> OnAnswerAdded;
    public event Action<int> OnAnswerOtherAdded;
    public event Action<int> OnAnswerRemoved;
    public event Action<int, string> OnViewpointSelected;
    public event Action<int, int, string> OnAnswerTextChanged;
    public void AddInitialAnswer();
}

public interface ISurveyQuestionBuilderUIGrid : ISurveyQuestionBuilderUI {
    public event Action<int> OnAddRow;
    public event Action<int> OnAddColumn;
}