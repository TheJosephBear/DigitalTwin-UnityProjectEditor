using SurveySystem;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestionView : MonoBehaviour
{
    private VisualElement _questionRoot;
    private QuestionType _questionType;
    private TemplateContainer _templateContainer;

    public QuestionView(VisualElement questionRoot, TemplateContainer templateContainer)
    {
        _questionRoot = questionRoot;
        _templateContainer = templateContainer;

        _questionType = (QuestionType)System.Enum.Parse(typeof(QuestionType), _templateContainer.name);
        Button addOptionButton = _questionRoot.Q<Button>("add-option-button");
        addOptionButton.clicked += AddOption;
    }

    void AddOption()
    {
        _questionRoot.Q<RadioButtonGroup>("options-list").Add(_templateContainer);

        switch (_questionType)
        {
            case QuestionType.MultipleChoiceSingle:
                
                // Logic to add option for multiple choice question
                break;
            case QuestionType.MultipleChoiceMultiple:
                // Logic to add option for multiple choice question
                break;
            case QuestionType.LinearScale:
                // Logic to add option for linear scale question
                break;
            default:
                Debug.LogWarning("Unsupported question type for adding options.");
                break;
        }
    }
}
