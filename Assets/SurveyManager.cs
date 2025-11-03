using QuestionnaireToolkit.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using QuestionnaireToolkit;
using TMPro;
using System;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;

public class SurveyManager : Singleton<SurveyManager> {

    public SurveyBuilder SurveyBuilderPrefab;
    
    QTQuestionnaireManager _qm;

    IQuestionAdapter _selectedQuestion;


    void Start() {
        // Load & instantiate prefab
        //    GameObject qmPrefab = Resources.Load<GameObject>("QuestionnaireToolkit/Prefabs/QuestionnaireManager");
    //    _qm = FindAnyObjectByType<QTQuestionnaireManager>();
    //    _qm.exportPath = System.IO.Path.Combine(Application.dataPath, "QuestionnaresSaved"); // Assets/QuestionnaresSaved

        //   AddPageAndQuestion();

        // Start manually
     //   _qm.StartQuestionnaire();
    }

    public void EnterSurveyBuilding() {
        SurveyBuilder builder = SceneLoadingManager.Instance
            .InstantiateObjectInScene(SurveyBuilderPrefab.gameObject)
            .GetComponent<SurveyBuilder>();
        builder.Initialize();
    }

    public void StartSurveyRuntime() {

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





    public void SetQuestionText(string newQuestionText) {
        _selectedQuestion.SetQuestionText(newQuestionText);
    }

    public int AddQuestionOption() {
        _selectedQuestion.AddOption();
        return _selectedQuestion.GetOptionsCount()-1;
    }

    public void SetOptionText(int optionIndex, string optionText) {
        _selectedQuestion.SetOptionText(optionIndex, optionText);
    }

    public void RemoveOption(int optionIndex) {
        _selectedQuestion.RemoveOption(optionIndex);
    }
    
    public List<QTOptionsData> GetOptionsData() {
        return _selectedQuestion.GetOptionsData();
    }

    public string GetQuestionText() {
        return _selectedQuestion.GetQuestionText();
    }

    public void SetQuestionTargetView(int idx) {
        _selectedQuestion.SetTargetView(ViewManager.Instance.GetViewPoints()[idx]);
    }

    public ViewPoint GetQuestionTargetView() {
        print("Getting the linear scale target vp: " + _selectedQuestion.GetTargetView());
        return _selectedQuestion.GetTargetView();
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
        linearScale.AddOption(scriptBased: true, a_value: "3", a_option: "meh");
        linearScale.AddOption(scriptBased: true, a_value: "4", a_option: "Trošku");
        linearScale.AddOption(scriptBased: true, a_value: "5", a_option: "Ranec");


    }



    QTQuestionPageManager GetSelectedPage() {
        return _qm.questionPages[_qm.selectedPage].GetComponent<QTQuestionPageManager>();
    }

    QTQuestionPageManager GetPageByIndex(int index) {
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
