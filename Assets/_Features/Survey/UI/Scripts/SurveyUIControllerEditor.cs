using System;
using System.Collections.Generic;
using System.Linq;
using SurveySystem;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Handles the Survey Builder UI logic.
/// Receives input events from addedQuestion and answer fields, as well as buttons,
/// and relays the changes to the <see cref="SurveyBuilder"/> for updating the survey data model.
/// (Tohle by možná mohl dělat svůj vlastní script) -> Manages the instantiation of addedQuestion UI elements based on templates and keeps track of added questions.
/// </summary>
public class SurveyUIControllerEditor : MonoBehaviour {

    private VisualElement _root;
    private SurveyManager _surveyManager;
    private SurveyBuilder _surveyBuilder; // Interface for data model
    private SurveyUIBuilder _surveyUIBuilder; // Script adding template instances to UI
    private SurveyQuestionUIEditor _currentlySelectedQuestion;

    private VisualElement _surveyHeaderContainer;
    private DropdownField _surveyCameraDropdown;
    private VisualElement _surveyCameraView;
    private VisualElement _surveyImageView;
    private List<SerializableViewPoint> _surveyViewPoints = new();

    void Awake() {
        _surveyUIBuilder = GetComponent<SurveyUIBuilder>();
        _root = gameObject.GetComponent<UIDocument>().rootVisualElement;

        // Save button
        var saveButton = _root.Q<Button>("save-btn");
        if (saveButton != null) saveButton.clicked += HandleSavePressed;
        // Exit button
        var exitButton = _root.Q<Button>("exit-btn");
        if (exitButton != null) exitButton.clicked += HandleExitPressed;
    }

    public void Initialize(SurveyBuilder surveyBuilder, SurveyManager manager) {
        _surveyBuilder = surveyBuilder;
        _surveyManager = manager;

        RegisterBaseSurveyInputs();
    }

    void RegisterBaseSurveyInputs() {
        var titleBar = _root.Q("survey-title-bar");
        _surveyHeaderContainer = titleBar?.parent ?? _root;

        var titleField = _surveyHeaderContainer.Q<TextField>("question-title");
        var descField = _surveyHeaderContainer.Q<TextField>("question-description");

        if (titleField != null) {
            titleField.RegisterValueChangedCallback(evt => {
                _surveyBuilder.SetSurveyName(evt.newValue);
            });
        }

        if (descField != null) {
            descField.RegisterValueChangedCallback(evt => {
                _surveyBuilder.SetSurveyDescription(evt.newValue);
            });
        }

        // Camera viewpoint dropdown & views
        _surveyCameraDropdown = _surveyHeaderContainer.Q<DropdownField>("camera-view-dropdown");
        _surveyCameraView = _surveyHeaderContainer.Q<VisualElement>("camera-view");
        _surveyImageView = _surveyHeaderContainer.Q<VisualElement>("question-image");

        if (_surveyCameraView != null) {
            _surveyCameraView.style.display = DisplayStyle.None;
            _surveyCameraView.style.backgroundImage = null;

            var removeViewBtn = _surveyCameraView.Q<Button>("remove-view");
            if (removeViewBtn != null) {
                removeViewBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    if (_surveyCameraDropdown != null && _surveyCameraDropdown.choices != null && _surveyCameraDropdown.choices.Count > 0) {
                        _surveyCameraDropdown.value = _surveyCameraDropdown.choices[0];
                    }
                });
            }

            var enhanceCamBtn = _surveyCameraView.Q<Button>("enhance-image");
            if (enhanceCamBtn != null) {
                enhanceCamBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    SurveyUIUtils.EnhanceImage(_surveyCameraView, _surveyUIBuilder?.FullscreenImageOverlayTemplate);
                });
            }
        }

        if (_surveyImageView != null) {
            _surveyImageView.style.display = DisplayStyle.None;
            _surveyImageView.style.backgroundImage = null;

            _surveyImageView.RegisterCallback<ClickEvent>(evt => {
                if (evt.target is Button btn && (btn.name == "enhance-image" || btn.name == "remove-image")) return;
                HandleSurveyImageUpload();
            });

            var removeImgBtn = _surveyImageView.Q<Button>("remove-image");
            if (removeImgBtn != null) {
                removeImgBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    _surveyBuilder.SetSurveyImageID("");
                    SetSurveyImageRender();
                });
            }

            var enhanceImgBtn = _surveyImageView.Q<Button>("enhance-image");
            if (enhanceImgBtn != null) {
                enhanceImgBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    SurveyUIUtils.EnhanceImage(_surveyImageView, _surveyUIBuilder?.FullscreenImageOverlayTemplate);
                });
            }
        }

        var imageButton = _surveyHeaderContainer.Q<Button>("image-button");
        if (imageButton != null) {
            imageButton.clicked += HandleSurveyImageUpload;
        }

        PopulateSurveyCameraDropdown();
    }

    void PopulateSurveyCameraDropdown() {
        if (_surveyCameraDropdown == null) return;

        var viewManager = FindAnyObjectByType<ViewManager>();
        _surveyViewPoints = viewManager?.GetSerializedViewPointsList() ?? new List<SerializableViewPoint>();

        var choices = new List<string> { "Žádný" };
        foreach (var vp in _surveyViewPoints) {
            choices.Add(vp.Name);
        }

        _surveyCameraDropdown.choices = choices;
        if (choices.Count > 0) {
            _surveyCameraDropdown.value = choices[0];
        }

        _surveyCameraDropdown.RegisterValueChangedCallback(evt => {
            int index = _surveyCameraDropdown.index - 1;

            if (index == -1) {
                _surveyBuilder.SetSurveyViewPointID("");
                if (_surveyCameraView != null) {
                    _surveyCameraView.style.backgroundImage = null;
                    _surveyCameraView.style.display = DisplayStyle.None;
                }
                SetSurveyImageRender();
            } else if (index >= 0 && index < _surveyViewPoints.Count) {
                string vpId = _surveyViewPoints[index].ID;
                _surveyBuilder.SetSurveyViewPointID(vpId);
                SetSurveyViewPointRender(vpId);
            }
        });
    }

    void HandleSurveyImageUpload() {
        ImageManager.Instance.AskForImageDialog((textureAsset) => {
            if (textureAsset == null || string.IsNullOrEmpty(textureAsset.ID)) {
                Debug.LogError("Upload failed: TextureAsset or ID is null");
                return;
            }

            _surveyBuilder.SetSurveyImageID(textureAsset.ID);
            SetSurveyImageRender();
        });
    }

    void SetSurveyImageRender() {
        if (_surveyImageView == null) return;

        Survey survey = _surveyBuilder.GetActiveSurvey();
        if (survey == null || string.IsNullOrEmpty(survey.ImageID)) {
            _surveyImageView.style.backgroundImage = null;
            _surveyImageView.style.display = DisplayStyle.None;
            return;
        }

        TextureAsset textureAsset = ImageManager.Instance.GetTextureAssetByID(survey.ImageID);
        if (textureAsset == null) {
            _surveyImageView.style.backgroundImage = null;
            _surveyImageView.style.display = DisplayStyle.None;
            return;
        }

        _surveyImageView.style.backgroundImage = Background.FromTexture2D((Texture2D)textureAsset.Texture);
        _surveyImageView.style.display = DisplayStyle.Flex;
    }

    void SetSurveyViewPointRender(string viewPointId) {
        if (_surveyCameraView == null) return;

        if (string.IsNullOrEmpty(viewPointId)) {
            _surveyCameraView.style.backgroundImage = null;
            _surveyCameraView.style.display = DisplayStyle.None;
            return;
        }

        RenderTexture rt = _surveyUIBuilder.CreateRenderTexture(viewPointId);
        if (rt != null) {
            _surveyCameraView.style.display = DisplayStyle.Flex;
            _surveyCameraView.style.backgroundImage = Background.FromRenderTexture(rt);
        }
    }

    #region Input handling

    #region Survey Building

    public void HandleQuestionAdded(string questionTypeString, int insertAtIndex = -1) {
        QuestionTypeMapping mapping = _surveyUIBuilder.questionUIMapping.GetMappingByQuestionType(questionTypeString);
        if (mapping == null) {
            Debug.LogError($"No mapping found for string: {questionTypeString}");
            return;
        }

        QuestionType questionTypeEnum = mapping.QuestionType;
        //    if (questionTypeEnum == QuestionType.ImageChoice) questionTypeEnum = QuestionType.MultipleChoiceSingle; // IMAGE CHOICE ZATÍM NEDÁVAT
        HandleQuestionAdded(questionTypeEnum, insertAtIndex);
    }

    public SurveyQuestionUIBase HandleQuestionAdded(QuestionType questionType, int insertAtIndex = -1) {
        QuestionBase newQuestion = _surveyBuilder.AddNewQuestion(questionType);
        return HandleExistingQuestionAdded(newQuestion, insertAtIndex);
    }

    public SurveyQuestionUIBase HandleExistingQuestionAdded(QuestionBase question, int insertAtIndex = -1, bool isDeserialized = false) {
        SurveyQuestionUIBase addedQuestionUI = _surveyUIBuilder.AddQuestionEditor(question, insertAtIndex: insertAtIndex, isDeserialized: isDeserialized);

        if (addedQuestionUI is SurveyQuestionUIEditor editorUI) {
            editorUI.OnQuestionSelected += HandleQuestionSelectionChanged;
            if (!isDeserialized) {
                HandleQuestionSelectionChanged(editorUI);
            }

            // Use a lambda to fetch the LATEST index from the UI list at the moment the event fires
            editorUI.OnQuestionDeleted += (id) => {
                int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(addedQuestionUI);
                HandleQuestionDeleted(dynamicIdx);
            };

            editorUI.OnQuestionMoved += (id, direction) => {
                int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(addedQuestionUI);
                HandleQuestionMoved(dynamicIdx, direction);
            };

            editorUI.OnTitleChanged += HandleQuestionTitleChanged;
            editorUI.OnDescriptionChanged += HandleQuestionDescriptionChanged;
       //     editorUI.OnQuestionDeleted += HandleQuestionDeleted;
      //      editorUI.OnQuestionMoved += HandleQuestionMoved;
             editorUI.OnUploadImage += HandleImageUpload;
             editorUI.OnRemoveImage += HandleRemoveImage;
             editorUI.OnViewpointSelected += HandleQuestionViewPointSelected;
            editorUI.OnMoveAnswer += HandleAnswerMoved;
            editorUI.OnToggleRequired += HandleRequiredChange;
        }

        if (addedQuestionUI is SurveyQuestionUIEditorGrid gridUI) {
            gridUI.OnAddRow += AddRow;
            gridUI.OnAddColumn += AddColumn;
            gridUI.OnRemoveRow += (qId, rowIdx) => _surveyBuilder.RemoveRow(qId, rowIdx);
            gridUI.OnRemoveColumn += (qId, colIdx) => _surveyBuilder.RemoveColumn(qId, colIdx);
            // Temporary - initial answer adding
            if (!isDeserialized) {
                AddRow(gridUI.QuestionID, gridUI.AddRow());
                AddColumn(gridUI.QuestionID, gridUI.AddColumn());
            }
        } else if (addedQuestionUI is SurveyQuestionUIEditorString builderUI) {
            builderUI.OnAnswerAdded += HandleAnswerAdded;
            builderUI.OnAnswerOtherAdded += HandleAnswerOtherAdded;
            builderUI.OnAnswerRemoved += HandleAnswerRemoved;

            if (!isDeserialized) {
                builderUI.AddInitialAnswer();
            }
        } else if (addedQuestionUI is SurveyQuestionUIEditorImage imageUI) {
            imageUI.OnAnswerImageChanged += HandleImageQuestionAnswerImageUpload;
            imageUI.OnAnswerAdded += HandleAddAnswerImage;
            imageUI.OnAnswerRemoved += HandleAnswerRemoved;

            if (!isDeserialized) {
                imageUI.AddInitialAnswer();
            }
        } else if (addedQuestionUI is SurveyQuestionUIEditorLinearScale scaleUI) {
            scaleUI.OnAnswerAdded += HandleAnswerAdded;
            scaleUI.OnAnswerRemoved += HandleAnswerRemoved;
            scaleUI.OnScaleTypeChanged += (qId, scaleType, min, max) => {
                _surveyBuilder.SetLinearScaleRange(qId, scaleType, min, max);
            };

            if (question is QuestionLinearScale linScale) {
                scaleUI.SetScaleRange(linScale.ScaleType, linScale.Min, linScale.Max);
            }

            if (!isDeserialized) {
                scaleUI.AddInitialAnswer();
            }
        }

        return addedQuestionUI;
    }

    void HandleRequiredChange(int questionID, bool isRequired) {
        _surveyBuilder.SetQuestionRequired(questionID, isRequired);
    }

    void AddRow(int questionID, SurveyAnswerUIEditorGrid answerUI) {
        _surveyBuilder.AddRow(questionID);
        answerUI.OnTextChanged += OnRowTextChanged;
    }

    void AddColumn(int questionID, SurveyAnswerUIEditorGrid answerUI) {
        _surveyBuilder.AddColumn(questionID);
        answerUI.OnTextChanged += OnColumnTextChanged;
    }

    void OnRowTextChanged(int questionID, int rowIdx, string text) {
        _surveyBuilder.SetRowText(questionID, rowIdx, text);
    }

    void OnColumnTextChanged(int questionID, int columnIdx, string text) {
        _surveyBuilder.SetColumnText(questionID, columnIdx, text);
    }

    public void HandleQuestionDeleted(int questionIndex) {
        print($"(Controller) Deleting index {questionIndex}");
        var questionUI = _surveyUIBuilder.GetQuestionAtIndex(questionIndex) as SurveyQuestionUIEditor;
        if (questionUI != null && _currentlySelectedQuestion == questionUI) {
            _currentlySelectedQuestion = null;
        }

        if (!_surveyUIBuilder.DeleteQuestion(questionIndex)) return;
        _surveyBuilder.RemoveQuestion(questionIndex);
    }

    public void HandleQuestionMoved(int questionIndex, int direction) {
        print($"(Controller) Moving index {questionIndex} in direction: {direction}");
        _surveyUIBuilder.MoveQuestion(questionIndex, direction);
        _surveyBuilder.MoveQuestion(questionIndex, direction);
    }

    void HandleAnswerMoved(int questionIndex, int answerIndex, int direction) {
        _surveyBuilder.MoveAnswer(questionIndex, answerIndex, direction);
    }

    public void HandleQuestionTitleChanged(int questionId, string newText) {
        _surveyBuilder.SetQuestionTitle(questionId, newText);
    }

    public void HandleQuestionDescriptionChanged(int questionId, string newText) {
        _surveyBuilder.SetQuestionDescription(questionId, newText);
    }

    public void HandleQuestionViewPointSelected(int questionID, string viewPointID) {
        _surveyBuilder.SetQuestionViewPointID(questionID, viewPointID);
    }

    public void HandleAnswerAdded(int questionId, SurveyAnswerUIBase answerUI) {
        _surveyBuilder.AddNewAnswerToQuestion(questionId);
        if (answerUI is SurveyAnswerUIEditorString answerEditor) {
            answerEditor.OnTextChanged += HandleAnswerTextChanged;
        } else if (answerUI is SurveyAnswerUIEditorLinearScale answerEditorScale) {
            answerEditorScale.OnTextChanged += HandleAnswerTextChanged;
        }
    }

    public void HandleAnswerOtherAdded(int questionId, SurveyAnswerUIBase answerUI) {
        _surveyBuilder.AddNewAnswerToQuestion(questionId, true);
        if (answerUI is SurveyAnswerUIEditorString answerEditor) {
            answerEditor.OnTextChanged += HandleAnswerTextChanged;
        }
    }

    public void HandleAnswerTextChanged(int questionId, int answerId, string newText) {
        _surveyBuilder.SetAnswerText(questionId, answerId, newText);
    }

    public void HandleAnswerRemoved(int questionId, int answerId) {
        _surveyBuilder.RemoveAnswer(questionId, answerId);
    }

    void HandleImageUpload(int questionIndex) {
        ImageManager.Instance.AskForImageDialog((textureAsset) => {
            // Validation: Ensure the asset and ID actually exist
            if (textureAsset == null || string.IsNullOrEmpty(textureAsset.ID)) {
                Debug.LogError("Upload failed: TextureAsset or ID is null");
                return;
            }

            Debug.Log($"Assigning ImageID: {textureAsset.ID} to question: {questionIndex}");

            // Update the Data Model
            _surveyBuilder.SetQuestionImageID(questionIndex, textureAsset.ID);

            // Update the UI
            _surveyUIBuilder.SetQuestionImage(questionIndex, textureAsset.ID);
        });
    }

    void HandleRemoveImage(int questionIndex) {
        Debug.Log($"Removing Image from question index: {questionIndex}");
        _surveyBuilder.SetQuestionImageID(questionIndex, "");
        _surveyUIBuilder.SetQuestionImage(questionIndex, "");
    }

    void HandleImageQuestionAnswerImageUpload(int questionID, int answerID, string imageID) {
        _surveyBuilder.SetAnswerImage(questionID, answerID, imageID);
    }

    void HandleAddAnswerImage(int questionID) {
        _surveyBuilder.AddNewAnswerToQuestion(questionID);
    }

    #endregion



    public void HandleSavePressed() {
        _surveyManager.SaveSurvey();
    }

    public void HandleExitPressed() {
        PopUp.Instance.AreYouSurePopUp((exit) => {
            if (exit) {
                _surveyManager.ExitSurvey();
            }
        });
    }

    #endregion

    private void HandleQuestionSelectionChanged(SurveyQuestionUIEditor selectedQuestion) {
        // 1. If the clicked question is already the active one, do nothing
        if (_currentlySelectedQuestion == selectedQuestion) return;

        // 2. Remove the USS class from the previously selected question
        if (_currentlySelectedQuestion != null && _currentlySelectedQuestion.QuestionElement != null) {
            _currentlySelectedQuestion.QuestionElement.RemoveFromClassList("active-question");
            _currentlySelectedQuestion.QuestionElement.RemoveFromClassList("new-question");
        }

        // 3. Assign the new selection and apply the USS class
        _currentlySelectedQuestion = selectedQuestion;

        if (_currentlySelectedQuestion != null && _currentlySelectedQuestion.QuestionElement != null) {
            _currentlySelectedQuestion.QuestionElement.AddToClassList("active-question");
        }
    }

    /// <summary>
    /// Builds the UI from data in the active survey
    /// </summary>
    public void DeserializeUI() {
        if (_surveyUIBuilder == null) _surveyUIBuilder = GetComponent<SurveyUIBuilder>();

        Survey survey = _surveyBuilder.GetActiveSurvey();
        // set title & description
        var titleField = _surveyHeaderContainer?.Q<TextField>("question-title") ?? _root.Q<TextField>("question-title");
        var descField = _surveyHeaderContainer?.Q<TextField>("question-description") ?? _root.Q<TextField>("question-description");
        if (titleField != null) titleField.SetValueWithoutNotify(survey.Name ?? "");
        if (descField != null) descField.SetValueWithoutNotify(survey.Description ?? "");

        // Restore survey viewpoint
        if (MainManagerBase.Instance != null && !string.IsNullOrEmpty(survey.ViewPointId)) {
            ViewPoint vp = MainManagerBase.Instance.ViewManager.GetViewPointByID(survey.ViewPointId);
            if (vp != null && _surveyCameraDropdown != null) {
                _surveyCameraDropdown.SetValueWithoutNotify(vp.Name);
                SetSurveyViewPointRender(survey.ViewPointId);
            } else if (_surveyCameraDropdown != null && _surveyCameraDropdown.choices != null && _surveyCameraDropdown.choices.Count > 0) {
                _surveyCameraDropdown.SetValueWithoutNotify(_surveyCameraDropdown.choices[0]);
            }
        } else if (_surveyCameraDropdown != null && _surveyCameraDropdown.choices != null && _surveyCameraDropdown.choices.Count > 0) {
            _surveyCameraDropdown.SetValueWithoutNotify(_surveyCameraDropdown.choices[0]);
        }

        // Restore survey image unconditionally
        SetSurveyImageRender();

        _surveyUIBuilder.ClearScrollviewContent();

        foreach (QuestionBase question in survey.GetAllQuestions()) {
            QuestionType questionType = question.QuestionType;
            SurveyQuestionUIBase questionUI = HandleExistingQuestionAdded(question, isDeserialized: true);
            questionUI.SetTitle(question.Title);
            questionUI.SetDescription(question.Description);
            questionUI.ImageID = question.ImageID;

            print("Calling required: " + question.IsRequired);
            (questionUI as SurveyQuestionUIEditor).SetRequired(question.IsRequired);

            // Set selected viewpoint
            if (MainManagerBase.Instance != null && !string.IsNullOrEmpty(question.ViewPointId)) {
                ViewPoint vp = MainManagerBase.Instance.ViewManager.GetViewPointByID(question.ViewPointId);
                if (vp != null) {
                    (questionUI as SurveyQuestionUIEditor).SetSelectedView(vp);
                }
            }

            // Always render question image unconditionally
            questionUI.SetImageRender();

            if (question is QuestionGridBase gridQuestion) {
                if (questionUI is SurveyQuestionUIEditorGrid gridUI) {
                    for (int i = 0; i < gridQuestion.GetColumnCount(); i++) {
                        var colUI = gridUI.AddColumn(gridQuestion.GetColumn(i));
                        colUI.OnTextChanged += OnColumnTextChanged;
                    }
                    for (int i = 0; i < gridQuestion.GetRowCount(); i++) {
                        var rowUI = gridUI.AddRow(gridQuestion.GetRow(i));
                        rowUI.OnTextChanged += OnRowTextChanged;
                    }
                }
            } else if (question is QuestionImageChoice imageQuestion) {
                if (questionUI is SurveyQuestionUIEditorImage imageUI) {
                    foreach (AnswerBase answer in imageQuestion.Answers) {
                        if (answer is AnswerImage imgAns) {
                            imageUI.AddAnswerWithImage(imgAns.GetImageId());
                        }
                    }
                }
            } else if (questionUI is SurveyQuestionUIEditorString stringQuestion) {
                foreach (AnswerBase answer in question.Answers) {
                    SurveyAnswerUIBase answerUIBase = questionUI.AddAnswer(answer.Text, answer.IsOther);
                    if(answerUIBase is SurveyAnswerUIEditorString answerEditorString)
                        answerEditorString.OnTextChanged += HandleAnswerTextChanged;
                }
            } else if (question is QuestionLinearScale linScaleQuestion && questionUI is SurveyQuestionUIEditorLinearScale scaleUI) {
                scaleUI.SetScaleRange(linScaleQuestion.ScaleType, linScaleQuestion.Min, linScaleQuestion.Max);
                foreach (AnswerBase answer in question.Answers) {
                    SurveyAnswerUIBase answerUIBase = scaleUI.AddAnswer(answer.Text);
                    if (answerUIBase is SurveyAnswerUIEditorLinearScale answerEditorScale)
                        answerEditorScale.OnTextChanged += HandleAnswerTextChanged;
                }
            }
        }

        _surveyUIBuilder.RefreshAddQuestionBars();
    }
}
