using QuestionnaireToolkit.Scripts;
using QuestionnaireToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;

public class SurveyBuilder : MonoBehaviour {

    QTQuestionnaireManager _qm;
    IQuestionAdapter _selectedQuestion;

    // Load the needed assets
    public void Initialize() {

    }

    // Select created question instance (works with UI calls)
    public void SelectQuestion(GameObject questionGO, QuestionItemsEnum type) {
        switch (type) {
            case QuestionItemsEnum.MultipleChoice:
                _selectedQuestion = new MultipleChoiceAdapter(questionGO.GetComponent<QTMultipleChoice>());
                break;

            case QuestionItemsEnum.LinearScale:
                _selectedQuestion = new LinearScaleAdapter(questionGO.GetComponent<QTLinearScale>());
                break;
        }
        print("New question selected! " + type);
    }

    #region Adding/Removing

    public void AddPageToQuestionnare() {
        _qm.CreatePage();
        _qm.ShowPage(_qm.questionPages.Count - 1); // Select page
    }

    // Adding new questions to the selected page
    public void AddNewQuestion(QuestionItemsEnum type) {
        QTQuestionPageManager selectedPage = GetSelectedPage();
        selectedPage.type = type;
        selectedPage.AddItem();
        selectedPage.selectedItem = selectedPage.questionItems[selectedPage.questionItems.Count - 1];

        SelectQuestion(selectedPage.selectedItem, type);
    }

    public int AddQuestionOption() {
        _selectedQuestion.AddOption();
        return _selectedQuestion.GetOptionsCount() - 1;
    }

    public void RemoveOption(int optionIndex) {
        _selectedQuestion.RemoveOption(optionIndex);
    }

    #endregion

    #region Setting

    public void SetQuestionnareName(string name) {
        // Název není souèást balíku
        // Název bude pøidán pøi redesignu
    }

    public void SetQuestionText(string newQuestionText) {
        _selectedQuestion.SetQuestionText(newQuestionText);
    }

    public void SetOptionText(int optionIndex, string optionText) {
        _selectedQuestion.SetOptionText(optionIndex, optionText);
    }

    public void SetQuestionTargetView(int idx) {
        _selectedQuestion.SetTargetView(ViewManager.Instance.GetViewPoints()[idx]);
    }

    #endregion

    #region Getting


    public string GetQuestionText() {
        return _selectedQuestion.GetQuestionText();
    }

    public List<QTOptionsData> GetOptionsData() {
        return _selectedQuestion.GetOptionsData();
    }

    public ViewPoint GetQuestionTargetView() {
        return _selectedQuestion.GetTargetView();
    }

    QTQuestionPageManager GetSelectedPage() {
        return _qm.questionPages[_qm.selectedPage].GetComponent<QTQuestionPageManager>();
    }

    QTQuestionPageManager GetPageByIndex(int index) {
        return _qm.questionPages[index].GetComponent<QTQuestionPageManager>();
    }

    #endregion

    #region Import/Export

    // Save questionnare into the project data
    public void SaveQuestionnare() {
        _qm.ExportPages();
    }

    // Load questionnare from the project data
    public void LoadQuestionnare() {

    }

    #endregion

}
