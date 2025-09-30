using QuestionnaireToolkit.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using QuestionnaireToolkit;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;

public class SurveyManager : Singleton<SurveyManager> {

    private QTQuestionnaireManager qm;

    void Start() {
        // Load & instantiate prefab
        //    GameObject qmPrefab = Resources.Load<GameObject>("QuestionnaireToolkit/Prefabs/QuestionnaireManager");
        qm = FindAnyObjectByType<QTQuestionnaireManager>();

        AddPageAndQuestion();



        // Start manually
        qm.StartQuestionnaire();
    }

    void AddPageAndQuestion() {
        qm.CreatePage();
        qm.ShowPage(0);
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
        print("Get selected page");
        print(qm);
        print(qm.questionPages[qm.selectedPage]);
        print(qm.questionPages[qm.selectedPage].GetComponent<QTQuestionPageManager>());
        return qm.questionPages[qm.selectedPage].GetComponent<QTQuestionPageManager>();
    }

    QTQuestionPageManager GetPageManagerByIndex(int index) {
        return qm.questionPages[index].GetComponent<QTQuestionPageManager>();
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
