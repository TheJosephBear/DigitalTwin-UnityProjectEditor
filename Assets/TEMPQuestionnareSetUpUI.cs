using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
using TMPro;
using UnityEngine;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;

public class TEMPQuestionnareSetUpUI : MonoBehaviour {

    public Transform AddedOptionListParentReff;
    public SurveyAddedOption AddedOptionUIPrefab;

    QuestionItemsEnum _addedQuestionType = QuestionItemsEnum.LinearScale;

    /*
    public void UpdateOptionList() {
        foreach (var (text, obj) in SurveyManager.Instance.GetOptionListLinear()) {
            Instantiate(AddedOptionUIPrefab, AddedOptionListParentReff).SetOptionName(text);
        }
    }
    */



    public void SaveQuestionnare() {
        SurveyManager.Instance.SaveQuestionnare();
    }

    public void LoadQuestionnare() {
        SurveyManager.Instance.LoadQuestionnare();
    }

    public void AddPageToQuestionnare() {
        SurveyManager.Instance.AddPageToQuestionnare();
    }

    public void AddQuestionToSelectedPage() {
        SurveyManager.Instance.AddNewQuestion(_addedQuestionType);
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
    }

    public void SetQuestionText(string text) {
        SurveyManager.Instance.SetQuestionText(text);
    }

    public void AddOptionToQuestion() {
        int questionIdx = SurveyManager.Instance.AddQuestionOption();
        // Add it to UI
        Instantiate(AddedOptionUIPrefab, AddedOptionListParentReff).GetComponent<SurveyAddedOption>().Initialize(
            name: "", 
            index: questionIdx, 
            rootUIReff: this
        );
    }

    public void SetOptionText(int optionIndex, string text) {
        SurveyManager.Instance.SetOptionText(optionIndex, text);
    }

    public void RemoveOption(int optionIndex) {
        SurveyManager.Instance.RemoveOption(optionIndex);
    }


}
