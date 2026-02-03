using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyController : Singleton<SurveyController>
{
    private VisualElement _root;
    private VisualElement _scrollViewContent;
    [SerializeField]
    private List<VisualTreeAsset> questionTemplates = new List<VisualTreeAsset>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _scrollViewContent = _root.Q<ScrollView>("survey-scroll-view").contentContainer;
    }

    public void AddQuestion(string questionType)
    {
        VisualTreeAsset questionTemplate = questionTemplates.Find(b => b.name == questionType);
        
        if (questionTemplate != null)
        {
            TemplateContainer questionInstance = questionTemplate.Instantiate();
            _scrollViewContent.Add(questionInstance);
        }
        else
        {
            _scrollViewContent.Add(new Label($"Question template '{questionType}' is missing"));
        }
        
        // Placeholder for adding question logic
        Debug.Log($"Question of type {questionType} added to the survey.");
    }
}
