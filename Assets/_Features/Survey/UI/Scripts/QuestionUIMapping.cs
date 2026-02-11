using SurveySystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

[Serializable]
public class QuestionAnswerUIMap
{
    public QuestionType questionType;
    public VisualTreeAsset answerUITemplate;
}

public class QuestionUIMapping : MonoBehaviour
{
    [SerializeField]
    private List<QuestionAnswerUIMap> questionAnswerMappings = new List<QuestionAnswerUIMap>();
    
    // Runtime dictionary for fast lookups
    private Dictionary<QuestionType, VisualTreeAsset> _mappingDictionary;
    
    private void Awake()
    {
        // Build dictionary from list
        _mappingDictionary = new Dictionary<QuestionType, VisualTreeAsset>();
        foreach (var mapping in questionAnswerMappings)
        {
            if (!_mappingDictionary.ContainsKey(mapping.questionType))
            {
                _mappingDictionary.Add(mapping.questionType, mapping.answerUITemplate);
            }
        }
    }
    
    // Method to get UI template for a question type
    public VisualTreeAsset GetAnswerUITemplate(QuestionType questionType)
    {
        if (_mappingDictionary == null) Awake();
        return _mappingDictionary.TryGetValue(questionType, out VisualTreeAsset template) ? template : null;
    }
    
    // Optional: Property to access dictionary (creates it if needed)
    public Dictionary<QuestionType, VisualTreeAsset> QuestionAnswerUIMapping
    {
        get
        {
            if (_mappingDictionary == null) Awake();
            return _mappingDictionary;
        }
    }
}
