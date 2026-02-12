using QuestionnaireToolkit.Scripts;
using QuestionnaireToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuestionnaireToolkit.Scripts.QTQuestionPageManager;
using System.Linq;

namespace SurveySystem {
    public class SurveyBuilder : Singleton<SurveyBuilder> {
        private Survey _activeSurvey;
        private int _nextId = 0;

        public Survey CreateNewSurvey() {
            _activeSurvey = new Survey();
            return _activeSurvey;
        }

        public void SetActiveSurvey(Survey survey) {
            _activeSurvey = survey;
        }

        public void SetSurveyName(string name) {
            _activeSurvey.Name = name;
        }

        public QuestionBase AddNewQuestion(QuestionType type) {
            QuestionBase question = type switch {
                QuestionType.MultipleChoiceSingle => new QuestionMultipleChoiceSingleAnswer(_nextId++),
                //       QuestionType.MultipleChoiceMultiple => new MultiChoiceMultiple(),
                _ => null
            };

            _activeSurvey.AddNewQuestion(question);
            return question;
        }

        public void RemoveQuestion(int idx) {
            _activeSurvey.RemoveQuestion(idx);
        }

        public void SetQuestionTitle(string title) {
            _activeSurvey.ActiveQuestion.Title = title;
        }

        public void SetQuestionTitle(QuestionBase question, string text) {
            question.Title = text;
        }

        public void SetQuestionTitle(int questionId, string text) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.Title = text;
        }

        public void SetQuestionDescription(string description) {
            _activeSurvey.ActiveQuestion.Description = description;
        }

        public void SetQuestionDescription(QuestionBase question, string text) {
            question.Description = text;
        }

        public void SetQuestionDescription(int questionId, string text) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.Description = text;
            DebugPrintSurvey();
        }

        public void AddNewAnswerToQuestion() {
            _activeSurvey.ActiveQuestion.AddNewAnswer();
        }
        public void AddNewAnswerToQuestion(QuestionBase question) {
            question.AddNewAnswer();
        }

        public void AddNewAnswerToQuestion(int questionId) {
            QuestionBase question = _activeSurvey.GetQuestionById(questionId);
            question.AddNewAnswer();
        }

        public void SetActiveAnswer(int idx) {
            _activeSurvey.ActiveQuestion.SetActiveAnswer(idx);
        }

        public void SetAnswerText(string text) {
            _activeSurvey.ActiveQuestion.ActiveAnswer.Text = text;
        }

        public void SetAnswerText(AnswerBase answer, string text) {
            answer.Text = text;
        }

        public void SetAnswerText(int questionId, int answerId, string text) {
            _activeSurvey.GetQuestionById(questionId).GetAnswerByIdx(answerId).Text = text;
        }

        public void RemoveAnswer(int idx) {
            _activeSurvey.ActiveQuestion.RemoveAnswer(idx);
        }

        public string ExportSurveyAsJson() {
            string jsonString = "";

            return jsonString;
        }



        void DebugPrintSurvey() {
            if (_activeSurvey == null) {
                print("No active survey.");
                return;
            }

            print($"Survey: {_activeSurvey.Name}");

            foreach (var question in _activeSurvey.Questions) {
                print($"Question [{question.Id}]");
                print($"  Title: {question.Title}");
                print($"  Description: {question.Description}");

                if (question.Answers == null || question.Answers.Count == 0) {
                    print("  Answers: <none>");
                    continue;
                }

                for (int i = 0; i < question.Answers.Count; i++) {
                    var answer = question.Answers[i];
                    print($"  Answer {i}: {answer.Text}");
                }
            }
        }
    }
}

/*
// I want to avoid errors before i get rid of the old code so here it stays for now
public class SurveyBuilder : MonoBehaviour {

    public GameObject QuestionnarePrefab;
    public GameObject ObstructorPrefab;
    public GameObject ControlPanelPrefab;

    public Vector3 QuestionnareCanvasOffset;

    QTQuestionnaireManager _qm;
    IQuestionAdapter _selectedQuestion;
    int _originalCullingMask;
    GameObject _questionnareInstance;
    GameObject _obstructorInstance;
    GameObject _controlPanelInstance;

    // Load the needed assets and setup
    public void Initialize() {
        // Position everything in front of the camera
        // (asset UI is in worldspace so we need to hide it)
        InstantiateQuestionnare();
        // Set camera to see only UI
        _originalCullingMask = Camera.main.cullingMask;
        Camera.main.cullingMask = 1 << LayerMask.NameToLayer("UI");

        _qm.StartQuestionnaire();
    }

    public void ExitSurveyBuilding() {
        // Restore original visible layers
        Camera.main.cullingMask = _originalCullingMask;
        _questionnareInstance.SetActive(false);
        _obstructorInstance.SetActive(false);
        _controlPanelInstance.SetActive(false);
        // Volat level editor je špatný, ale to se opraví po refaktorizaci editor managera
        EditorManager.Instance.ChangeEditorMode(EditorState.Freecam);
    }

    void InstantiateQuestionnare() {
        Camera cam = Camera.main;
        if (cam == null) return;

        Quaternion uiRotation = cam.transform.rotation;

        if (_questionnareInstance == null) {
            _questionnareInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(QuestionnarePrefab, cam.transform.position);
        } else {
            _questionnareInstance.SetActive(true);  
        }

        _questionnareInstance.transform.rotation = uiRotation;
        _questionnareInstance.transform.position += _questionnareInstance.transform.TransformVector(QuestionnareCanvasOffset);

        if (_obstructorInstance == null) {
            _obstructorInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(ObstructorPrefab, _questionnareInstance.transform.position);
        } else {
            _obstructorInstance.SetActive(true);
        }

        _obstructorInstance.transform.rotation = uiRotation;
        _obstructorInstance.transform.position -= cam.transform.forward * -2f;

        if (_controlPanelInstance == null) {
            _controlPanelInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(ControlPanelPrefab.gameObject);
            _controlPanelInstance.GetComponent<SurveyControlPanel>().Initialize(this);
        }

        _qm = _questionnareInstance.GetComponent<QTQuestionnaireManager>();
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
        // Název není součást balíku
        // Název bude přidán při redesignu
    }

    public void SetQuestionText(string newQuestionText) {
        _selectedQuestion.SetQuestionText(newQuestionText);
    }

    public void SetOptionText(int optionIndex, string optionText) {
        _selectedQuestion.SetOptionText(optionIndex, optionText);
    }

    public void SetQuestionTargetView(int idx) {
        _selectedQuestion.SetTargetView(EditorManager.Instance.ViewManager.GetViewPoints()[idx]);
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

 */
