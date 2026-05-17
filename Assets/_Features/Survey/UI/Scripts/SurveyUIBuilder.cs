using Cinemachine;
using SurveySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class SurveyUIBuilder : MonoBehaviour {

    public QuestionUIMapping questionUIMapping;
    [SerializeField]
    public VisualTreeAsset GridCollumnTemplate;
    public Texture AsteriskTexture;
    public Texture AsteriskCrossedTexture;

    [SerializeField]
    private VisualTreeAsset addQuestionBarTemplate;

    private VisualElement _root;
    private ScrollView _scrollView;
    private VisualElement _scrollViewContent;

    // Question adding //
    private List<SurveyQuestionUIBase> _addedQuestions = new List<SurveyQuestionUIBase>();
    private List<TemplateContainer> _addQuestionBars = new List<TemplateContainer>();

    private List<QuestionType> QuestionTypesUsingStringUI = new List<QuestionType>{
        QuestionType.MultipleChoiceSingle,
        QuestionType.MultipleChoiceMultiple,
        QuestionType.Paragraph,
        QuestionType.ShortAnswer,
        QuestionType.LinearScale,
    };

    private List<QuestionType> QuestionTypesUsingGridUI = new List<QuestionType>{
        QuestionType.MultipleChoiceGrid,
        QuestionType.CheckboxGrid
    };

    void Awake() {
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;
        _scrollView = _root.Q<ScrollView>("survey-scroll-view");
        _scrollViewContent = _root.Q<ScrollView>("survey-scroll-view").contentContainer;

        // Add the initial bar at the start (before any questions)
        RefreshAddQuestionBars();
    }

    /// <summary>
    /// Rebuilds the scroll view content so there is one add-addedQuestion bar before each addedQuestion
    /// and one trailing bar after the last addedQuestion: [bar, q0, bar, q1, bar, ..., barN].
    /// </summary>
    public void RefreshAddQuestionBars() {
        // _scrollViewContent.Clear();
        // Clear scroll view content except for the first item (the title item)
        for (int i = _scrollViewContent.childCount - 1; i > 0; i--) {
            _scrollViewContent.RemoveAt(i);
        }

        if (addQuestionBarTemplate == null) {
            Debug.LogWarning("addQuestionBarTemplate is not assigned on SurveyBuildingUI.");
            return;
        }

        // Remove existing bars from the hierarchy
        foreach (var bar in _addQuestionBars) {
            bar.RemoveFromHierarchy();
        }
        _addQuestionBars.Clear();

        // Remove addedQuestion instances from hierarchy so we can re-insert them in order
        foreach (var question in _addedQuestions) {
            if (question.QuestionElement != null) {
                question.QuestionElement.RemoveFromHierarchy();
            }
        }

        // Re-insert: bar, [addedQuestion, bar] * N
        for (int i = 0; i <= _addedQuestions.Count; i++) {
            TemplateContainer bar = addQuestionBarTemplate.Instantiate();
            _addQuestionBars.Add(bar);
            _scrollViewContent.Add(bar);

            // Wire up the add-addedQuestion button on this bar
            int insertIndex = i; // capture for lambda
            TemplateContainer capturedBar = bar; // capture for lambda
            var addButton = bar.Q<Button>("add-question-button");
            if (addButton != null) {
                addButton.clicked += () => AddQuestion.Instance.OpenModalAtIndex(insertIndex, capturedBar);
            }

            if (i < _addedQuestions.Count && _addedQuestions[i].QuestionElement != null) {
                _scrollViewContent.Add(_addedQuestions[i].QuestionElement);
            }
        }
    }

    public void SetQuestionImage(int questionIndex, string textureAssetID) {
        if (questionIndex < 0 || questionIndex >= _addedQuestions.Count) {
            Debug.LogError("Question index out of bounds!");
            return;
        }

        Debug.Log($"Setting and activating image in builder. Index: {questionIndex}, ID: {textureAssetID}");

        // Explicitly set the ID first
        _addedQuestions[questionIndex].ImageID = textureAssetID;

        // Now call the render
        _addedQuestions[questionIndex].SetImageRender();
    }

    void ScrollToAddedElement(TemplateContainer addedElement) {
        if (addedElement == null) return;

        _scrollView.schedule.Execute(() =>
        {
            _scrollView.ScrollTo(addedElement);
        }).ExecuteLater(1);
    }

    public SurveyQuestionUIBase AddQuestionEditor(QuestionBase addedQuestion, bool isDeserialized, int insertAtIndex = -1, VisualTreeAsset template = null) {
        print("Adding question in builder, type is: " + addedQuestion.QuestionType);
        if (template == null) {
            //Template not provided, look it up
            QuestionTypeMapping mapping = questionUIMapping.GetMappingByQuestionType(addedQuestion.QuestionType);

            if (mapping == null) {
                Debug.LogError($"No mapping found for enum: {addedQuestion.QuestionType}");
                return null;
            }

            template = mapping.QuestionTemplate;
        }

        return CreateQuestion(addedQuestion, template, insertAtIndex: insertAtIndex, isDeserialized: isDeserialized);
    }

    private SurveyQuestionUIBase CreateQuestion(QuestionBase addedQuestion, VisualTreeAsset template, bool isDeserialized, int insertAtIndex = -1) {
        QuestionType questionType = addedQuestion.QuestionType;
        TemplateContainer questionInstance;

        if (template != null) {
            questionInstance = template.Instantiate();
        } else {
            questionInstance = new TemplateContainer();
            questionInstance.Add(new Label($"Question template for '{questionType}' is missing"));
        }

        SurveyQuestionUIBase questionUI = null;

        if(QuestionTypesUsingStringUI.Contains(questionType)) {
            questionUI = new SurveyQuestionUIEditorString(
                questionInstance,
                addedQuestion.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this,
                isDeserialized: isDeserialized
            );
        } else if(QuestionTypesUsingGridUI.Contains(questionType)) {
            questionUI = new SurveyQuestionUIEditorGrid(
                questionInstance,
                addedQuestion.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this,
                isDeserialized: isDeserialized
            );
        } else if (questionType == QuestionType.ImageChoice) {
            questionUI = new SurveyQuestionUIEditorImage(
                questionInstance,
                addedQuestion.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this
            );
        }

        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }

        RefreshAddQuestionBars();
        questionUI.RegisterInputs();
        ScrollToAddedElement(questionInstance);

        return questionUI;
    }
    
    public SurveyQuestionUIViewer AddQuestionViewer(QuestionBase questionBase, int insertAtIndex = -1) {
        QuestionType questionType = questionBase.QuestionType;
        TemplateContainer questionInstance;

        QuestionTypeMapping mapping = questionUIMapping.GetMappingByQuestionType(questionType);

        if (mapping == null) {
            Debug.LogError($"No mapping found for enum: {questionType}");
            return null;
        }

        VisualTreeAsset template = mapping.QuestionTemplate;

        questionInstance = template != null
            ? template.Instantiate()
            : new TemplateContainer();

        if (template == null) {
            questionInstance.Add(new Label($"Missing template for '{questionType}'"));
        }

        SurveyQuestionUIViewer questionUI = null;

        if (QuestionTypesUsingStringUI.Contains(questionType)) {
            questionUI = new SurveyQuestionUIViewerString(
                questionInstance,
                questionBase.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this
            );
        } else if (QuestionTypesUsingGridUI.Contains(questionType)) {
            questionUI = new SurveyQuestionUIViewerGrid(
                questionInstance,
                questionBase.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this
            );
        } else if (questionType == QuestionType.ImageChoice) {
            questionUI = new SurveyQuestionUIViewerImage(
                questionInstance,
                questionBase.Id,
                questionType,
                FindAnyObjectByType<ViewManager>()?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>(),
                this
            );
        }

        _scrollViewContent.Add(questionInstance);

        foreach (var bar in _addQuestionBars) {
            bar.RemoveFromHierarchy();
        }
        _addQuestionBars.Clear();


        if (insertAtIndex < 0 || insertAtIndex >= _addedQuestions.Count) {
            _addedQuestions.Add(questionUI);
        } else {
            _addedQuestions.Insert(insertAtIndex, questionUI);
        }

        return questionUI;
    }

    public void MoveQuestion(int questionIndex, int direction) {

        print($"(Builder UI) Moving index {questionIndex}");
        int targetIndex = questionIndex + direction;
        print($"(Builder UI) Target index {targetIndex}");
        print($"(Builder UI) return check? {(targetIndex < 0 || targetIndex >= _addedQuestions.Count)}");
        if (targetIndex < 0 || targetIndex >= _addedQuestions.Count) return;

        var temp = _addedQuestions[questionIndex];
        _addedQuestions[questionIndex] = _addedQuestions[targetIndex];
        _addedQuestions[targetIndex] = temp;

        print($"(Builder UI) Swapped in list and refreshing UI!");

        RefreshAddQuestionBars();
    }

    public bool DeleteQuestion(int questionIndex) {
        if (questionIndex < 0 || questionIndex >= _addedQuestions.Count) return false;

        _addedQuestions[questionIndex].QuestionElement?.RemoveFromHierarchy();
        _addedQuestions.RemoveAt(questionIndex);

        RefreshAddQuestionBars();   
        return true;
    }

    /// <summary>Returns the current index of the given addedQuestion in the list, or -1 if not found.</summary>
    public int GetQuestionIndex(SurveyQuestionUIBase questionUI) {
        return _addedQuestions.IndexOf(questionUI);
    }

    public void ClearScrollviewContent() {
        foreach (var question in _addedQuestions) {
            if (question.QuestionElement != null) {
                question.QuestionElement.RemoveFromHierarchy();
            }
        }
    }

    #region View rendering

    private Dictionary<string, RenderTexture> _createdTextures = new Dictionary<string, RenderTexture>();

    public RenderTexture CreateRenderTexture(string viewPointId) {
        var viewManager = GameObject.FindFirstObjectByType<ViewManager>();
        var viewPoint = viewManager.GetViewPointByID(viewPointId);

        viewPoint.Activate(); // Position the camera at the viewpoint

        Camera unityCamera = Camera.main;
        CinemachineBrain brain = unityCamera.GetComponent<CinemachineBrain>();
        brain.ManualUpdate();
        float originalBlendSpeed = brain.m_DefaultBlend.m_Time;
        brain.m_DefaultBlend.m_Time = 0f;


        // 1. Create a new texture for this specific question
        // Note: We use a Depth of 24 for a standard 3D render
        RenderTexture questionRT = new RenderTexture(1024, 1024, 24);
        questionRT.name = "Question_Capture_" + viewPointId;
        questionRT.Create();

        // 2. "Hijack" the camera for exactly one frame
        RenderTexture previousRT = unityCamera.targetTexture; // Save existing state
        unityCamera.targetTexture = questionRT;

        // Manually force the camera to render RIGHT NOW
        unityCamera.Render();

        // 3. Release the camera immediately
        unityCamera.targetTexture = previousRT;

        // 5. Track this texture 
        if(_createdTextures.ContainsKey(viewPointId)) _createdTextures.Remove(viewPointId);
        _createdTextures.Add(viewPointId, questionRT);

        StartCoroutine(ClearRenderTextureStuffNextFrame(viewPoint, brain, originalBlendSpeed));

        return questionRT;
    }

    IEnumerator ClearRenderTextureStuffNextFrame(ViewPoint vp, CinemachineBrain brain, float ogSpeed) {
        yield return new WaitForEndOfFrame();
        vp.Deactivate();
        brain.m_DefaultBlend.m_Time = ogSpeed;
    }

    public void ClearAllQuestionTextures() {
        foreach (var item in _createdTextures.Values) {
            if (item != null) {
                item.Release();
                Destroy(item); // Important to prevent memory leaks in the editor
            }
        }
        _createdTextures.Clear(); // Clear everything at once after the loop
    }

    #endregion
}
