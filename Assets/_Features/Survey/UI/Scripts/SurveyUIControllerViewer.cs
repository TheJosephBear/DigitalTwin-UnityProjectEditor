using Cinemachine;
using SurveySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyUIControllerViewer : MonoBehaviour {

    private VisualElement _root;
    private SurveyManager _surveyManager;
    private SurveyBuilder _surveyBuilder; // Interface for data model
    private SurveyResponseManager _responseManager; // Handles response data model
    private SurveyUIBuilder _surveyUIBuilder; // Script adding template instances to UI

    private Dictionary<int, SurveyQuestionUIBase> _questionUICache = new();
    private List<QuestionBase> _questions = new();
    private int _currentPage = 0; // 0 = Intro Page, 1..N = Questions
    private VisualElement _firstPageElement;

    public void Initialize(SurveyBuilder surveyBuilder, SurveyResponseManager responseManager, SurveyManager manager) {
        _surveyBuilder = surveyBuilder;
        _responseManager = responseManager;
        _surveyManager = manager;

        Survey survey = _surveyBuilder.GetActiveSurvey();
        _questions = survey != null ? survey.GetAllQuestions() : new List<QuestionBase>();

        _surveyUIBuilder = GetComponent<SurveyUIBuilder>();
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        #region Button setup

        var toggleButton = _root.Q<Button>("toggle-btn");
        if (toggleButton != null) toggleButton.clicked += HandleTogglePressed;

        var prevButton = _root.Q<Button>("previous-btn");
        if (prevButton != null) prevButton.clicked += HandlePreviousPressed;

        var nextButton = _root.Q<Button>("next-btn");
        if (nextButton != null) nextButton.clicked += HandleNextPressed;

        #endregion

        _firstPageElement = _root.Q<VisualElement>("survey-first-page");

        if (_surveyUIBuilder != null) {
            _surveyUIBuilder.ClearAddQuestionBars();
        }

        var introQuestionImage = _firstPageElement?.Q<VisualElement>("question-image");
        var enhanceImgBtn = introQuestionImage?.Q<Button>("enhance-image");
        if (enhanceImgBtn != null) {
            enhanceImgBtn.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation();
                SurveyUIUtils.EnhanceImage(introQuestionImage, _surveyUIBuilder?.FullscreenImageOverlayTemplate);
            });
        }

        if (survey != null) {
            SetupIntroPage(survey);
        }

        DisplayPage(0);
    }

    void SetupIntroPage(Survey survey) {
        if (_firstPageElement == null) return;

        var titleLabel = _firstPageElement.Q<Label>("survey-title");
        if (titleLabel != null) titleLabel.text = survey.Name ?? "";

        var descLabel = _firstPageElement.Q<Label>("question-description");
        if (descLabel != null) descLabel.text = survey.Description ?? "";

        var questionImage = _firstPageElement.Q<VisualElement>("question-image");
        if (questionImage != null) {
            if (!string.IsNullOrEmpty(survey.ImageID)) {
                TextureAsset textureAsset = ImageManager.Instance.GetTextureAssetByID(survey.ImageID);
                if (textureAsset != null) {
                    questionImage.style.display = DisplayStyle.Flex;
                    questionImage.style.backgroundImage = Background.FromTexture2D((Texture2D)textureAsset.Texture);
                } else {
                    questionImage.style.display = DisplayStyle.None;
                }
            } else {
                questionImage.style.display = DisplayStyle.None;
            }
        }
    }

    #region Input handling

    void HandleTogglePressed() {
        SurveyManager.Instance.ExitSurvey();
    }

    void HandleNextPressed() {
        if (_currentPage < _questions.Count) {
            DisplayPage(_currentPage + 1);
            SurveyManager.Instance.SaveAnswers();
        }
    }

    void HandlePreviousPressed() {
        if (_currentPage > 0) {
            DisplayPage(_currentPage - 1);
            SurveyManager.Instance.SaveAnswers();
        }
    }

    public void HandleAnswerSelected(int questionId, int answerId, bool isSelected) {
        print("ANSWER SELECTED");
        _responseManager.RegisterAnswer(questionId, answerId, isSelected);
    }

    public void HandleAnswerTextFilled(int questionId, int answerId, string newText) {
        print("TEXT FILLED");
        _responseManager.RegisterAnswer(questionId, answerId, true, newText);
    }

    #endregion

    void DisplayPage(int pageIndex) {
        _currentPage = pageIndex;
        int totalQuestions = _questions.Count;

        var pageCountLabel = _root.Q<Label>("page-count-label");
        if (pageCountLabel != null) {
            pageCountLabel.text = $"{_currentPage}/{totalQuestions}";
        }

        ClearQuestionFromUI();

        if (_currentPage == 0) {
            if (_firstPageElement != null) {
                _firstPageElement.style.display = DisplayStyle.Flex;
            }

            Survey survey = _surveyBuilder.GetActiveSurvey();
            if (survey != null) {
                SetupIntroPage(survey);
                if (!string.IsNullOrEmpty(survey.ViewPointId)) {
                    StartCoroutine(ShowViewCoroutine(survey.ViewPointId));
                }
            }
        } else {
            if (_firstPageElement != null) {
                _firstPageElement.style.display = DisplayStyle.None;
            }

            int questionIndex = _currentPage - 1;
            if (questionIndex >= 0 && questionIndex < _questions.Count) {
                QuestionBase currentQuestion = _questions[questionIndex];
                SurveyQuestionUIBase addedQuestionUI = AddQuestionToUI(currentQuestion);
                addedQuestionUI.SetImageRender();

                if (!string.IsNullOrEmpty(currentQuestion.ViewPointId)) {
                    StartCoroutine(ShowViewCoroutine(currentQuestion.ViewPointId));
                }
            }
        }
    }

    IEnumerator ShowViewCoroutine(string viewPointId) {
        if (MainManagerBase.Instance == null || string.IsNullOrEmpty(viewPointId)) yield break;

        ViewManager viewManager = MainManagerBase.Instance.ViewManager;
        if (viewManager == null) yield break;

        ViewPoint vp = viewManager.GetViewPointByID(viewPointId);
        if (vp == null) yield break;

        if (EditorManager.Instance != null && EditorManager.Instance.EditorCameraManager != null) {
            EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(true);
        }

        viewManager.DeactivateViewPoint();
        viewManager.SetActiveViewPoint(vp);
        viewManager.ActivateViewPoint();

        yield return null;

        CinemachineBrain brain = FindAnyObjectByType<CinemachineBrain>();

        if (brain != null && brain.ActiveBlend != null) {
            float blendTime = brain.ActiveBlend.Duration;
            yield return new WaitForSeconds(blendTime + 0.05f);
        } else {
            yield return null;
        }

        if (EditorManager.Instance != null && EditorManager.Instance.EditorCameraManager != null) {
            EditorManager.Instance.EditorCameraManager.ToggleCinemachineBrain(false);
        }
    }

    SurveyQuestionUIBase AddQuestionToUI(QuestionBase questionBase) {
        // Check if we already created this UI before
        if (_questionUICache.TryGetValue(questionBase.Id, out SurveyQuestionUIBase existingUI)) {
            existingUI.QuestionElement.style.display = DisplayStyle.Flex;
            return existingUI;
        }

        // If not in cache, create it for the first time
        SurveyQuestionUIBase questionUI = _surveyUIBuilder.AddQuestionViewer(questionBase);
        questionUI.SetTitle(questionBase.Title);
        questionUI.SetDescription(questionBase.Description);
        questionUI.ImageID = questionBase.ImageID;

        questionUI.SetQuestionPosition(_currentPage);
        var mapping = _surveyUIBuilder.questionUIMapping.GetMappingByQuestionType(questionBase.QuestionType);
        if (mapping != null) {
            questionUI.SetQuestionType(mapping.DisplayName);
        }

        if (questionBase is QuestionGridBase gridQuestion && questionUI is SurveyQuestionUIViewerGrid gridUI) {
            for (int i = 0; i < gridQuestion.GetColumnCount(); i++) {
                gridUI.AddColumn(gridQuestion.GetColumn(i));
            }

            for (int i = 0; i < gridQuestion.GetRowCount(); i++) {
                gridUI.AddRow(gridQuestion.GetRow(i));
            }

            gridUI.OnGridAnswerSelected += (qId, row, col, val) => {
                if (val) _responseManager.RegisterGridAnswer(qId, row, col);
            };
        } else if (questionUI is SurveyQuestionUIViewerString stringUI) {
            stringUI.OnAnswerSelected += HandleAnswerSelected;
            stringUI.OnAnswerTextFilled += HandleAnswerTextFilled;
            foreach (AnswerBase answer in questionBase.Answers) {
                questionUI.AddAnswer(answer.Text, answer.IsOther);
            }
        } else if (questionUI is SurveyQuestionUIViewerImage imageUI) {
            imageUI.OnAnswerSelected += HandleAnswerSelected;
            foreach (AnswerBase answer in questionBase.Answers) {
                if (answer is AnswerImage imageAnswer) {
                    imageUI.AddAnswer(imageAnswer.GetImageId());
                }
            }
        } else if (questionBase is QuestionLinearScale linScaleQuestion && questionUI is SurveyQuestionUIViewerLinearScale scaleUI) {
            scaleUI.SetScaleRange(linScaleQuestion.ScaleType, linScaleQuestion.Min, linScaleQuestion.Max);
            scaleUI.OnScaleValueChanged += (qId, rowIdx, val) => {
                _responseManager.RegisterScaleAnswer(qId, rowIdx, val);
            };
            foreach (AnswerBase answer in questionBase.Answers) {
                scaleUI.AddAnswer(answer.Text);
            }
        }

        // Add to cache
        _questionUICache.Add(questionBase.Id, questionUI);
        return questionUI;
    }

    void ClearQuestionFromUI() {
        // Hide all cached questions
        foreach (var ui in _questionUICache.Values) {
            ui.QuestionElement.style.display = DisplayStyle.None;
        }
    }
}
