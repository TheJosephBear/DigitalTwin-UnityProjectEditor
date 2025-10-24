using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
using TMPro;
using UnityEngine;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;

public class TEMPQuestionnareSetUpUI : MonoBehaviour {

    public TMP_InputField QuestionTextInputFieldReff;
    public TMP_InputField OptionTextInputFieldReff;
    public Transform AddedOptionListParentReff;
    public SurveyAddedOption AddedOptionUIPrefab;

    QuestionItemsEnum _addedQuestionType = QuestionItemsEnum.LinearScale;



    public void UpdateOptionList() {
        foreach (var (text, obj) in SurveyManager.Instance.GetOptionListLinear()) {
            Instantiate(AddedOptionUIPrefab, AddedOptionListParentReff).SetOptionName(text);
        }
    }




    public void SaveQuestionnare() {
        SurveyManager.Instance.SaveQuestionnare();
    }

    public void LoadQuestionnare() {
        SurveyManager.Instance.LoadQuestionnare();
    }

    public void AddPageToQuestionnare() {
        SurveyManager.Instance.AddPageToQuestionnare();
    }

    public void AddQuestionToSelectedPageLinearScale() {
        string text = QuestionTextInputFieldReff.text;
        if (text == "") {
            text = "Defaultní otázkový text";
        }
        switch (_addedQuestionType) {
            case QuestionItemsEnum.LinearScale:
                SurveyManager.Instance.AddQuestionToSelectedPageLinearScale(text);
                break;
            case QuestionItemsEnum.MultipleChoice:
                SurveyManager.Instance.AddQuestionToSelectedPageMultipleChoice(text);
                break;
        }
    }

    public void SetAddedQuestionType(int idx) {
        switch (idx) {
            case 0:
                _addedQuestionType = QuestionItemsEnum.LinearScale;
                break;
            case 1:
                _addedQuestionType = QuestionItemsEnum.MultipleChoice;
                break;
        }
        print("_addedQuestionType is now " + _addedQuestionType);
    }

    public void SetLinearScaleQuestion() {
        switch (_addedQuestionType) {
            case QuestionItemsEnum.LinearScale:
                SurveyManager.Instance.SetLinearScaleQuestion(QuestionTextInputFieldReff.text);
                break;
            case QuestionItemsEnum.MultipleChoice:
            //    SurveyManager.Instance.Set(QuestionTextInputFieldReff.text);
                break;
        }
    }

    public void AddLinearScaleOption() {
        switch (_addedQuestionType) {
            case QuestionItemsEnum.LinearScale:
                SurveyManager.Instance.AddLinearScaleOption(OptionTextInputFieldReff.text);
                break;
            case QuestionItemsEnum.MultipleChoice:
                SurveyManager.Instance.AddMultipleChoiceOption(OptionTextInputFieldReff.text);
                break;
        }
    }
}
