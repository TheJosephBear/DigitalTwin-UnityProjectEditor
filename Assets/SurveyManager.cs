using QuestionnaireToolkit.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using QuestionnaireToolkit;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;
using TMPro;

public class SurveyManager : Singleton<SurveyManager> {

    QTQuestionnaireManager _qm;
    QTLinearScale _selectedLinearScale;
    QTMultipleChoice _selectedMultipleChoice;

    void Start() {
        // Load & instantiate prefab
        //    GameObject qmPrefab = Resources.Load<GameObject>("QuestionnaireToolkit/Prefabs/QuestionnaireManager");
        _qm = FindAnyObjectByType<QTQuestionnaireManager>();
        _qm.exportPath = System.IO.Path.Combine(Application.dataPath, "QuestionnaresSaved"); // Assets/QuestionnaresSaved

        //   AddPageAndQuestion();




        // Start manually
        _qm.StartQuestionnaire();
    }

    public void CreateNewQuestionnare() {
        // Inicializace
        _qm = FindAnyObjectByType<QTQuestionnaireManager>(); // tøeba, idk for now
    }

    public void SetQuestionnareName(string name) {
        // Název není souèást balíku
        // Název bude pøidán pøi redesignu
    }

    public void AddPageToQuestionnare() {
        _qm.CreatePage();
        _qm.ShowPage(_qm.questionPages.Count-1); // Select page
    }

    public void AddQuestionToSelectedPageLinearScale(string question) {
        QTQuestionPageManager selectedPage = GetSelectedPage();
        selectedPage.question = question;
        selectedPage.type = QuestionItemsEnum.LinearScale;
        selectedPage.AddItem();
        selectedPage.selectedItem = selectedPage.questionItems[selectedPage.questionItems.Count - 1];
        QTLinearScale linearScale = selectedPage.selectedItem.GetComponent<QTLinearScale>();
        _selectedLinearScale = linearScale;
        //    _selectedLinearScale.question = question;
    }

    public void SetLinearScaleQuestion(string question) {
        _selectedLinearScale.question = question;
    }

    public void AddLinearScaleOption(string optionText) {
        _selectedLinearScale.AddOption(scriptBased: true, a_value: "1", a_option: optionText);
    }

    public void AddQuestionToSelectedPageMultipleChoice(string question) {
        QTQuestionPageManager selectedPage = GetSelectedPage();
        selectedPage.question = question;
        selectedPage.type = QuestionItemsEnum.MultipleChoice;
        selectedPage.AddItem();
        selectedPage.selectedItem = selectedPage.questionItems[selectedPage.questionItems.Count - 1];
        QTMultipleChoice multipleChoice = selectedPage.selectedItem.GetComponent<QTMultipleChoice>();
        _selectedMultipleChoice = multipleChoice;
    }

    public void AddMultipleChoiceOption(string optionText) {
        _selectedMultipleChoice.answerOption = optionText;
        _selectedMultipleChoice.answerValue = _selectedMultipleChoice.options.Count.ToString();
        _selectedMultipleChoice.AddOption();
    }

    public List<(string text, GameObject option)> GetOptionListLinear() {
        var optionPairs = new List<(string, GameObject)>();

        foreach (GameObject option in _selectedLinearScale.options) {
            string optionText = option.GetComponentInChildren<TextMeshProUGUI>().text;
            optionPairs.Add((optionText, option));
        }

        return optionPairs;
    }


    public void GetOptionListMultiple() {

    }

    public void SaveQuestionnare() {
        _qm.ExportPages();
    }

    public void LoadQuestionnare() {
        _qm.importPath = _qm.exportPath + "/MyQuestionnaire.json";
        _qm.ImportPages();
    }



    void AddPageAndQuestion() {
        _qm.CreatePage();
        _qm.ShowPage(0);
  //      qm.selectedPage = 0;
        QTQuestionPageManager selectedPage = GetSelectedPage();
        selectedPage.AddItem(i_type: QuestionItemsEnum.LinearScale);
        selectedPage.selectedItem = selectedPage.questionItems[0];
        QTLinearScale linearScale = selectedPage.selectedItem.GetComponent<QTLinearScale>();
        linearScale.question = "Wawwaaaaa";
        linearScale.AddOption(scriptBased: true, a_value: "1", a_option: "Vùbec");
        linearScale.AddOption(scriptBased: true, a_value: "2", a_option: "Trošku");
        linearScale.AddOption(scriptBased: true, a_value: "3", a_option: "Prcám");
        linearScale.AddOption(scriptBased: true, a_value: "4", a_option: "Trošku");
        linearScale.AddOption(scriptBased: true, a_value: "5", a_option: "Ranec");


    }



    QTQuestionPageManager GetSelectedPage() {
    /*    print("Get selected page");
        print(_qm);
        print("The idx of selected page is: " + _qm.selectedPage);
        print(_qm.questionPages[_qm.selectedPage]);
        print(_qm.questionPages[_qm.selectedPage].GetComponent<QTQuestionPageManager>());*/
        return _qm.questionPages[_qm.selectedPage].GetComponent<QTQuestionPageManager>();
    }

    QTQuestionPageManager GetPageManagerByIndex(int index) {
        return _qm.questionPages[index].GetComponent<QTQuestionPageManager>();
    }

    // Save questionnaire structure/settings as JSON
    public void SaveQuestionnaireSettings(string filePath) {
        // Built-in export
    //    qm.ExportQuestionnaire(filePath);
        Debug.Log($"Questionnaire exported to {filePath}");
    }

    // Load questionnaire settings from JSON
    public void LoadQuestionnaireSettings(string filePath) {
   //    qm.ImportQuestionnaire(filePath);
        Debug.Log($"Questionnaire imported from {filePath}");
    }

    // Handle results when questionnaire finishes
    private void OnFinished() {
        string jsonResults = GetResultsAsJson();
        string path = Path.Combine(Application.persistentDataPath, "Results.json");
        File.WriteAllText(path, jsonResults);
        Debug.Log($"Results saved to {path}");
    }

    // Collect results manually and serialize to JSON
    private string GetResultsAsJson() {
        var resultData = new System.Collections.Generic.Dictionary<string, object>();
/*
        foreach (var page in qm.Pages) {
            foreach (var item in page.Items) {
                string header = item.HeaderName;
                object answer = item.GetValue(); // Each item type has value (string, int, list, etc.)
                resultData[header] = answer;
            }
        }
*/
        return JsonUtility.ToJson(new SerializationWrapper(resultData), true);
    }

    // Helper wrapper for dictionary -> JSON
    [System.Serializable]
    private class SerializationWrapper {
        public System.Collections.Generic.Dictionary<string, object> dict;
        public SerializationWrapper(System.Collections.Generic.Dictionary<string, object> d) { dict = d; }
    }


}
