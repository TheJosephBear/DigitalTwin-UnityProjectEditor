using System;
using System.Collections.Generic;
using SurveySystem;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyViewingUI : MonoBehaviour {
    public List<QuestionTypeMapping> QuestionTypeMapping = new List<QuestionTypeMapping>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}

[Serializable]
public class QuestionTypeMapping {
    public QuestionType EnumValue;
    public string StringValue;
    public VisualTreeAsset Template;
    public VisualTreeAsset AnswerTemplate;
}
