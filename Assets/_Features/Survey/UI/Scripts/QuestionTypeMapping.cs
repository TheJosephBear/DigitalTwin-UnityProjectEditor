using System;
using SurveySystem;
using UnityEngine.UIElements;

[Serializable]
public class QuestionTypeMapping {
    public QuestionType EnumValue;
    public string StringValue;
    public VisualTreeAsset Template;
    public VisualTreeAsset AnswerTemplate;
}
