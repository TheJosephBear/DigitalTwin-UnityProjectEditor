using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
using TMPro;
using UnityEngine;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;

public class TEMPQuestionnareSetUpUI : MonoBehaviour {

    public Transform AddedOptionListParentReff;
    public SurveyAddedOption AddedOptionUIPrefab;

    public TMP_InputField QuestionTextReff;



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
        AddQuestionOptionToUIList(questionIdx);
    }

    public void SetOptionText(int optionIndex, string text) {
        SurveyManager.Instance.SetOptionText(optionIndex, text);
    }

    public void RemoveOption(int optionIndex) {
        SurveyManager.Instance.RemoveOption(optionIndex);
    }
    private void OnEnable() {
        UIClickableManager.Instance.OnUIClicked += HandleUIClick;
    }

    private void OnDisable() {
        UIClickableManager.Instance.OnUIClicked -= HandleUIClick;
    }

    private void HandleUIClick(List<GameObject> clickedObject) {

        QTLinearScale linScale;
        QTMultipleChoice multiChoice;

        string questionText = "";
        GameObject selectedGO;
        QuestionItemsEnum type;

        foreach (GameObject go in clickedObject) {

            if(go.GetComponent<QTLinearScale>() != null) {
                questionText = go.GetComponent<QTLinearScale>().question;
                selectedGO = go;
                type = QuestionItemsEnum.LinearScale;

                FillUIWithQuestionData(selectedGO, type);

            } else if (go.GetComponent<QTMultipleChoice>() != null) {
                questionText = go.GetComponent<QTMultipleChoice>().question;
                selectedGO = go;
                type = QuestionItemsEnum.MultipleChoice;

                FillUIWithQuestionData(selectedGO, type);

            }
        }
    }

    void FillUIWithQuestionData(GameObject selectedGO, QuestionItemsEnum type) {
        SurveyManager manager = SurveyManager.Instance;
        manager.SelectQuestion(selectedGO, type);
        List<QTOptionsData> data = SurveyManager.Instance.GetOptionsData();

        QuestionTextReff.SetTextWithoutNotify(manager.GetQuestionText());
        ClearQuestionOptionList();
        foreach (QTOptionsData item in data) {
            AddQuestionOptionToUIList(item.idx, item.questionText);
        }
    }

    void AddQuestionOptionToUIList(int questionIdx, string optionText = "") {
        Instantiate(AddedOptionUIPrefab, AddedOptionListParentReff).GetComponent<SurveyAddedOption>().Initialize(
            name: optionText,
            index: questionIdx,
            rootUIReff: this
        );
    }

    void ClearQuestionOptionList() {
        Utilities.KillAllChildren(AddedOptionListParentReff);
    }
}
