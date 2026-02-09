using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;

public class SurveyControlPanel : MonoBehaviour {
    /*
    public Transform AddedOptionListParentReff;
    public SurveyAddedOption AddedOptionUIPrefab;
    public SurveyBuilder surveyBuilder;

    public TMP_InputField QuestionTextReff;
    public TMP_Dropdown ViewPointDropdownReff;



    QuestionItemsEnum _addedQuestionType = QuestionItemsEnum.LinearScale;

    /*
    public void UpdateOptionList() {
        foreach (var (text, obj) in SurveyManager.Instance.GetOptionListLinear()) {
            Instantiate(AddedOptionUIPrefab, AddedOptionListParentReff).SetOptionName(text);
        }
    }
    */
    /*
    private void Start() {
        UIClickableManager.Instance.OnUIClicked += HandleUIClick;
        EditorManager.Instance.ViewManager.OnViewPointAddedEvent.AddListener(HandleViewPointCreated);
        UpdateViewDropdown();
    }

    public void Initialize(SurveyBuilder sb) {
        surveyBuilder = sb;
    }

    public void OnExit() {
        surveyBuilder.ExitSurveyBuilding();
    }

    public void OnSave() {
        surveyBuilder.SaveQuestionnare();
    }

    public void OnLoad() {
        surveyBuilder.LoadQuestionnare();
    }

    public void OnAddPage() {
        surveyBuilder.AddPageToQuestionnare();
    }

    public void OnAddQuestion() {
        surveyBuilder.AddNewQuestion(_addedQuestionType);
        FillUIWithQuestionData(_addedQuestionType);
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
        surveyBuilder.SetQuestionText(text);
    }

    public void AddOptionToQuestion() {
        int questionIdx = surveyBuilder.AddQuestionOption();
        AddQuestionOptionToUIList(questionIdx);
    }

    public void SetOptionText(int optionIndex, string text) {
        surveyBuilder.SetOptionText(optionIndex, text);
    }

    public void RemoveOption(int optionIndex) {
        surveyBuilder.RemoveOption(optionIndex);
    }

    public void SetQuestionTargetView(int idx) {
        surveyBuilder.SetQuestionTargetView(idx);
    }

    public void UpdateViewDropdown() {

        ViewPointDropdownReff.ClearOptions();
        List<string> optionNames = new List<string>();

        foreach (var vp in EditorManager.Instance.ViewManager.GetViewPoints()) {
            optionNames.Add(vp.Name);
        }

        ViewPointDropdownReff.AddOptions(optionNames);

        if (optionNames.Count > 0)
            ViewPointDropdownReff.value = 0;

        ViewPointDropdownReff.RefreshShownValue();
    }


    private void OnDisable() {
        UIClickableManager.Instance.OnUIClicked -= HandleUIClick;
        EditorManager.Instance.ViewManager.OnViewPointAddedEvent.RemoveListener(HandleViewPointCreated);
    }

    private void HandleViewPointCreated(ViewPoint vp) {
        UpdateViewDropdown();
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
        surveyBuilder.SelectQuestion(selectedGO, type);
        List<QTOptionsData> data = surveyBuilder.GetOptionsData();
        // Question text
        QuestionTextReff.SetTextWithoutNotify(surveyBuilder.GetQuestionText());
        // Options
        ClearQuestionOptionList();
        foreach (QTOptionsData item in data) {
            AddQuestionOptionToUIList(item.idx, item.questionText);
        }
        // Viewpoint
        ViewPoint vp = surveyBuilder.GetQuestionTargetView();
        if (vp != null) {
            int indexToSelect = ViewPointDropdownReff.options.FindIndex(option => option.text == vp.Name);
            ViewPointDropdownReff.SetValueWithoutNotify(indexToSelect);
        }
    }

    void FillUIWithQuestionData(QuestionItemsEnum type) {
        List<QTOptionsData> data = surveyBuilder.GetOptionsData();

        QuestionTextReff.SetTextWithoutNotify(surveyBuilder.GetQuestionText());
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
    */
}
