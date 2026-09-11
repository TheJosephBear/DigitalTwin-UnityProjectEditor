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
    private VisualElement _thankYouPageElement;
    private VisualElement _bottomBar;
    private Button _prevButton;
    private Button _nextButton;
    private Label _pageCountLabel;
    private bool _isSubmitted = false;

    public void Initialize(SurveyBuilder surveyBuilder, SurveyResponseManager responseManager, SurveyManager manager) {
        _surveyBuilder = surveyBuilder;
        _responseManager = responseManager;
        _surveyManager = manager;
        _isSubmitted = false;

        Survey survey = _surveyBuilder.GetActiveSurvey();
        _questions = survey != null ? survey.GetAllQuestions() : new List<QuestionBase>();

        _surveyUIBuilder = GetComponent<SurveyUIBuilder>();
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        #region Button setup

        var toggleButton = _root.Q<Button>("toggle-btn");
        if (toggleButton != null) toggleButton.clicked += HandleTogglePressed;

        _bottomBar = _root.Q<VisualElement>("bottom-bar");

        _prevButton = _root.Q<Button>("previous-btn");
        if (_prevButton != null) _prevButton.clicked += HandlePreviousPressed;

        _nextButton = _root.Q<Button>("next-btn");
        if (_nextButton != null) _nextButton.clicked += HandleNextPressed;

        _pageCountLabel = _root.Q<Label>("page-count-label");

        #endregion

        _firstPageElement = _root.Q<VisualElement>("survey-first-page");
        _thankYouPageElement = _root.Q<VisualElement>("survey-thank-you-page");

        var thankYouCloseBtn = _thankYouPageElement?.Q<Button>("thank-you-close-btn");
        if (thankYouCloseBtn != null) {
            thankYouCloseBtn.clicked += () => {
                SurveyManager.Instance.ExitSurvey();
            };
        }

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

        if (HasIntroPage(survey)) {
            DisplayPage(0);
        } else if (_questions.Count > 0) {
            DisplayPage(1);
        } else {
            DisplayPage(0);
        }
    }

    private bool HasIntroPage(Survey survey) {
        if (survey == null) return false;
        return !string.IsNullOrWhiteSpace(survey.Name) && !string.IsNullOrWhiteSpace(survey.Description);
    }

    void SetupIntroPage(Survey survey) {
        if (_firstPageElement == null) return;

        var titleLabel = _firstPageElement.Q<Label>("survey-title");
        if (titleLabel != null) titleLabel.text = survey.Name ?? "";

        var descLabel = _firstPageElement.Q<Label>("question-description");
        if (descLabel != null) {
            string trimmed = survey.Description?.Trim() ?? string.Empty;
            descLabel.text = trimmed;
            descLabel.style.display = string.IsNullOrEmpty(trimmed) ? DisplayStyle.None : DisplayStyle.Flex;
        }

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
        if (_isSubmitted) return;

        if (_currentPage < _questions.Count) {
            DisplayPage(_currentPage + 1);
            SurveyManager.Instance.SaveAnswers();
        } else if (_currentPage == _questions.Count && _questions.Count > 0) {
            // Last question page reached: submit survey!
            SubmitSurvey();
        } else if (_currentPage == 0 && _questions.Count == 0) {
            // 0 questions, intro page only
            SubmitSurvey();
        }
    }

    void SubmitSurvey() {
        _isSubmitted = true;
        SurveyManager.Instance.SaveAnswers();
        SurveyManager.Instance.UploadSurveyAnswers(success => {
            if (success) {
                Debug.Log("[Viewer] Survey successfully submitted to server.");
            } else {
                Debug.LogWarning("[Viewer] Server submission completed with warning/error.");
            }
        });
        DisplayThankYouPage();
    }

    void HandlePreviousPressed() {
        if (_isSubmitted) return;

        int minPage = HasIntroPage(_surveyBuilder.GetActiveSurvey()) ? 0 : 1;
        if (_currentPage > minPage) {
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
        _isSubmitted = false;
        Survey survey = _surveyBuilder.GetActiveSurvey();
        bool hasIntro = HasIntroPage(survey);

        if (!hasIntro && pageIndex == 0 && _questions.Count > 0) {
            pageIndex = 1;
        }

        _currentPage = pageIndex;
        int totalQuestions = _questions.Count;

        ClearQuestionFromUI();

        if (_thankYouPageElement != null) {
            _thankYouPageElement.style.display = DisplayStyle.None;
        }

        if (_bottomBar != null) {
            _bottomBar.style.display = DisplayStyle.Flex;
        }

        // Update buttons and labels
        UpdateNavigationUI(totalQuestions, hasIntro);

        if (_currentPage == 0) {
            if (_firstPageElement != null) {
                _firstPageElement.style.display = DisplayStyle.Flex;
            }

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

    void UpdateNavigationUI(int totalQuestions, bool hasIntro) {
        if (_pageCountLabel != null) {
            _pageCountLabel.style.display = DisplayStyle.Flex;
            if (_currentPage == 0) {
                _pageCountLabel.text = totalQuestions > 0 ? $"0/{totalQuestions}" : "Úvod";
            } else {
                _pageCountLabel.text = $"{_currentPage}/{totalQuestions}";
            }
        }

        int minPage = hasIntro ? 0 : 1;
        if (_prevButton != null) {
            _prevButton.style.display = DisplayStyle.Flex;
            _prevButton.style.visibility = _currentPage > minPage ? Visibility.Visible : Visibility.Hidden;
        }

        if (_nextButton != null) {
            _nextButton.style.display = DisplayStyle.Flex;
            _nextButton.style.backgroundColor = StyleKeyword.Null;
            _nextButton.style.color = StyleKeyword.Null;
            bool isLastQuestionPage = (totalQuestions > 0 && _currentPage == totalQuestions) || (totalQuestions == 0);

            if (isLastQuestionPage) {
                if (!_nextButton.ClassListContains("submit-btn")) {
                    _nextButton.AddToClassList("submit-btn");
                }
                _nextButton.text = "Odeslat";
            } else {
                if (_nextButton.ClassListContains("submit-btn")) {
                    _nextButton.RemoveFromClassList("submit-btn");
                }
                _nextButton.text = string.Empty;
            }
        }
    }

    void DisplayThankYouPage() {
        _currentPage = _questions.Count + 1;
        ClearQuestionFromUI();

        if (_firstPageElement != null) {
            _firstPageElement.style.display = DisplayStyle.None;
        }

        if (_thankYouPageElement != null) {
            _thankYouPageElement.style.display = DisplayStyle.Flex;
        }

        if (_bottomBar != null) {
            _bottomBar.style.display = DisplayStyle.None;
        }

        if (_prevButton != null) {
            _prevButton.style.display = DisplayStyle.None;
        }

        if (_nextButton != null) {
            _nextButton.style.display = DisplayStyle.None;
        }

        if (_pageCountLabel != null) {
            _pageCountLabel.text = "Dokončeno";
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
