using System;
using SurveySystem;
using UnityEngine.UIElements;

[Serializable]
public class QuestionTypeMapping {
    public QuestionType QuestionType;
    public string DisplayName;
    public VisualTreeAsset QuestionTemplate;
    public VisualTreeAsset AnswerTemplate;
}
