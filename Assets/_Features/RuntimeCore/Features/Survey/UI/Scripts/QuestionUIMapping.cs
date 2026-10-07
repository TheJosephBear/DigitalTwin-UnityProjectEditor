using SurveySystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

[CreateAssetMenu(fileName = "QuestionUIMapping", menuName = "Survey/Question UI Mapping", order = 1)]
public class QuestionUIMapping : ScriptableObject {
    [SerializeField]
    private List<QuestionTypeMapping> questionTypeMappings = new List<QuestionTypeMapping>();
    public IReadOnlyList<QuestionTypeMapping> QuestionTypeMappings => questionTypeMappings;

    // Runtime dictionary for fast lookups
    private Dictionary<QuestionType, QuestionTypeMapping> _mappingDictionary;

    public Dictionary<QuestionType, QuestionTypeMapping> QuestionAnswerUIMapping {
        get {
            if (_mappingDictionary == null) InitializeDictionary();
            return _mappingDictionary;
        }
    }

    public QuestionTypeMapping GetMappingByDisplayName(string displayName) {
        foreach (var mapping in questionTypeMappings) {
            if (string.Equals(mapping.DisplayName, displayName, StringComparison.OrdinalIgnoreCase)) {
                return mapping;
            }
        }
        return null;
    }

    public QuestionTypeMapping GetMappingByQuestionType(QuestionType questionType) {
        if (_mappingDictionary == null) InitializeDictionary();
        return _mappingDictionary.TryGetValue(questionType, out var mapping) ? mapping : null;
    }

    public QuestionTypeMapping GetMappingByQuestionType(string questionType) {
        if (_mappingDictionary == null) InitializeDictionary();
        if (Enum.TryParse<QuestionType>(questionType, true, out var parsedType)) {
            return _mappingDictionary.TryGetValue(parsedType, out var mapping) ? mapping : null;
        }
        return null;
    }

    #region Dictionary for fast lookups

    private void InitializeDictionary() {
        // Build dictionary from list
        _mappingDictionary = new Dictionary<QuestionType, QuestionTypeMapping>();
        foreach (var mapping in questionTypeMappings) {
            if (!_mappingDictionary.ContainsKey(mapping.QuestionType)) {
                _mappingDictionary.Add(mapping.QuestionType, mapping);
            }
        }
    }

    // Method to get UI template for a question type
    public VisualTreeAsset GetAnswerUITemplate(QuestionType questionType) {
        if (_mappingDictionary == null) InitializeDictionary();
        return _mappingDictionary.TryGetValue(questionType, out var mapping) ? mapping.AnswerTemplate : null;
    }
    #endregion
}
