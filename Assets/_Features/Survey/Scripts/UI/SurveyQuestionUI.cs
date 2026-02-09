using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;

public class SurveyQuestionUI
{
    public int _questionID;
    private SurveyBuildingUI _surveyBuildingUIReff;
    private VisualElement _root;

    public SurveyQuestionUI(VisualElement root, int questionId, SurveyBuildingUI surveyBuildingUI) {
        _root = root;
        _questionID = questionId;
        _surveyBuildingUIReff = surveyBuildingUI;
        RegisterInputs();
    }

    private void RegisterInputs() {
        var textField = _root.Q<TextField>("question-title-field");
        var answerField = _root.Q<TextField>("question-description");

        textField.RegisterValueChangedCallback(evt => {
            _surveyBuildingUIReff.HandleQuestionTitleChanged(_questionID, evt.newValue);
        });

        answerField.RegisterValueChangedCallback(evt => {
            _surveyBuildingUIReff.HandleQuestionDescriptionChanged(_questionID, evt.newValue);
        });
    }
}
