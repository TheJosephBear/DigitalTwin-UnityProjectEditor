using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;
using System.Collections.Generic;
using System;

public abstract class SurveyQuestionUIEditor : SurveyQuestionUIBase {

    #region Fields & Properties

    // Modal tracking
    protected VisualElement _currentlyOpenModal;
    protected VisualElement _originalParent;
    protected int _originalIndex = -1;
    protected bool _isRequired = false;

    #endregion

    #region Events

    #region Public events

    public event Action<int, string> OnTitleChanged;
    public event Action<int, string> OnDescriptionChanged;
    public event Action<int> OnQuestionDeleted;
    public event Action<int, int> OnQuestionMoved;
    public event Action<int, string> OnViewpointSelected;
    public event Action<int> OnUploadImage;
    public event Action<int, bool> OnToggleRequired;
    public event Action<int, int, int> OnMoveAnswer;
    public event Action<SurveyQuestionUIEditor> OnQuestionSelected;
    public event Action<int> OnRemoveImage;

    #endregion

    #region Internal events

    protected Action _onMoveUp;
    protected Action _onMoveDown;
    protected Action _onDelete;
    protected Action _onUpload;

    #endregion

    #endregion

    public SurveyQuestionUIEditor(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder)
        : base(root, questionId, questionType, viewPoints, uiBuilder) {

    }

    #region Interface for editing the question

    // Make it virtual or abstract - check the other add answer function and how different it is (this is for code calls, theo ther is for ui calls)
    public override SurveyAnswerUIBase AddAnswer(string answerText, bool isOther = false) {
        if (_optionsList == null || _answerTemplate == null) {
            Debug.LogWarning("Missing options list or template!");
            return null;
        }

        TemplateContainer element = _answerTemplate.Instantiate();

        int index = _addedAnswers.Count;

        if (isOther) {
            _optionsList.Add(element);
        } else {
            if (_otherAnswerUI != null) {
                int insertIndex = _optionsList.IndexOf(_otherAnswerUI.AnswerElement);
                _optionsList.Insert(insertIndex, element);
            } else {
                _optionsList.Add(element);
            }
        }

        TextField tf = FindTextFieldRecursive(element);
        if (tf != null)
            tf.value = answerText;

        SurveyAnswerUIBase answerUI = CreateAnswerUI(element, index, isOther);

        if (isOther) {
            var optContainer = element.Q<VisualElement>("option-container") ?? element;
            optContainer.AddToClassList("option-container--other");

            var radioButton = element.Q<CustomRadioButton>();
            if (radioButton != null) {
                radioButton.Placeholder = "Jiná odpověď...";
            }
            var toggleButton = element.Q<CustomToggleButton>();
            if (toggleButton != null) {
                toggleButton.Placeholder = "Jiná odpověď...";
            }
            _otherAnswerUI = answerUI;
        } else {
            _addedAnswers.Add(answerUI);
        }

        RecalculateAnswerIndices();

        return answerUI;
    }

    public void SetSelectedView(ViewPoint viewPoint) {
        var dropdown = _root.Q<DropdownField>("camera-view-dropdown");
        if (dropdown == null || viewPoint == null) return;
        dropdown.value = viewPoint.Name;
        SetViewPointRender(viewPoint.ID);
    }

    public Tuple<int, string> GetSelectedViewName() {
        var dropdown = _root.Q<DropdownField>("camera-view-dropdown");
        return new Tuple<int, string>(dropdown.index, dropdown.value);
    }

    public void ClearSelectedView() {
        var dropdown = _root.Q<DropdownField>("camera-view-dropdown");
        if (dropdown != null && dropdown.choices != null && dropdown.choices.Count > 0) {
            dropdown.value = dropdown.choices[0];
        }
    }

    public void ToggleRequired() {
        SetRequired(!_isRequired);
    }

    public void SetRequired(bool required) {
        Debug.Log("Setting required to " + required);
        // Inner flag
        _isRequired = required;

        var toggle = _root.Q<Toggle>("required-toggle");
        if (toggle != null) {
            toggle.SetValueWithoutNotify(required);
        }

        // Event
        OnToggleRequired?.Invoke(QuestionID, _isRequired);
    }

    #endregion

    #region UI Input Registration

    protected override void RegisterButtons() {
        // required toggle switch pill
        var requiredToggle = _root.Q<Toggle>("required-toggle");
        if (requiredToggle != null) {
            requiredToggle.RegisterValueChangedCallback(evt => {
                _isRequired = evt.newValue;
                OnToggleRequired?.Invoke(QuestionID, _isRequired);
            });
        }

        var requiredLabel = _root.Q<Label>("required-label");
        if (requiredLabel != null) {
            requiredLabel.RegisterCallback<ClickEvent>(evt => {
                ToggleRequired();
            });
        }

        // Active question on pointer down or focus (keeps active when typing)
        _root.RegisterCallback<PointerDownEvent>(evt => {
            OnQuestionSelected?.Invoke(this);
        });

        _root.RegisterCallback<FocusInEvent>(evt => {
            OnQuestionSelected?.Invoke(this);
        });

        // Enhance / Remove image buttons
        var cameraView = _root.Q<VisualElement>("camera-view");
        if (cameraView != null) {
            var enhanceCamBtn = cameraView.Q<Button>("enhance-image");
            if (enhanceCamBtn != null) {
                enhanceCamBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    var currentCamView = _root.Q<VisualElement>("camera-view");
                    EnhanceImage(currentCamView);
                });
            }

            var removeViewBtn = cameraView.Q<Button>("remove-view");
            if (removeViewBtn != null) {
                removeViewBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    ClearSelectedView();
                });
            }
        }

        var questionImage = _root.Q<VisualElement>("question-image");
        if (questionImage != null) {
            questionImage.RegisterCallback<ClickEvent>(evt => {
                if (evt.target is Button btn && (btn.name == "enhance-image" || btn.name == "remove-image")) {
                    return;
                }
                int index = _surveyUIBuilder.GetQuestionIndex(this);
                OnUploadImage?.Invoke(index);
            });

            var enhanceImgBtn = questionImage.Q<Button>("enhance-image");
            if (enhanceImgBtn != null) {
                enhanceImgBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    var currentQuestionImg = _root.Q<VisualElement>("question-image");
                    EnhanceImage(currentQuestionImg);
                });
            }

            var removeImgBtn = questionImage.Q<Button>("remove-image");
            if (removeImgBtn != null) {
                removeImgBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    int index = _surveyUIBuilder.GetQuestionIndex(this);
                    OnRemoveImage?.Invoke(index);
                });
            }
        }
    }

    /// <summary>Registers all UI callbacks.</summary>
    protected override void RegisterTextInputs() {
        _root.Q<TextField>("question-title")?.RegisterValueChangedCallback(
            evt => OnTitleChanged?.Invoke(QuestionID, evt.newValue));

        _root.Q<TextField>("question-description")?.RegisterValueChangedCallback(
            evt => OnDescriptionChanged?.Invoke(QuestionID, evt.newValue));
    }

    protected override void RegisterDropdown() {
        var dropdown = _root.Q<DropdownField>("camera-view-dropdown");
        PopulateCameraViewDropdown(dropdown);

        dropdown?.RegisterValueChangedCallback(evt => {
            int index = dropdown.index - 1;

            if (index == -1) {
                OnViewpointSelected?.Invoke(QuestionID, "");
                var cameraView = _root.Q<VisualElement>("camera-view");
                if (cameraView != null) {
                    cameraView.style.backgroundImage = null;
                    cameraView.style.display = DisplayStyle.None;
                }
                SetImageRender();
            }

            if (index >= 0 && index < _viewPoints.Count) {
                OnViewpointSelected?.Invoke(QuestionID, _viewPoints[index].ID);
                SetViewPointRender(_viewPoints[index].ID);
            }
        });
    }

    #endregion

    #region Answer Reordering

    public virtual void MoveAnswerUp(int index) {
        if (index <= 0 || index >= _addedAnswers.Count) return;
        SwapAnswers(index, index - 1);

        int questionIndex = _surveyUIBuilder.GetQuestionIndex(this);
        OnMoveAnswer?.Invoke(questionIndex, index, -1);
    }

    public virtual void MoveAnswerDown(int index) {
        if (index < 0 || index >= _addedAnswers.Count - 1) return;
        SwapAnswers(index, index + 1);

        int questionIndex = _surveyUIBuilder.GetQuestionIndex(this);
        OnMoveAnswer?.Invoke(questionIndex, index, 1);
    }

    protected virtual void SwapAnswers(int a, int b) {
        (_addedAnswers[a], _addedAnswers[b]) = (_addedAnswers[b], _addedAnswers[a]);

        RefreshAnswerOrder();
        RecalculateAnswerIndices();
    }

    protected virtual void RefreshAnswerOrder() {
        _optionsList.Clear();

        foreach (var a in _addedAnswers)
            _optionsList.Add(a.AnswerElement);

        if (_otherAnswerUI != null)
            _optionsList.Add(_otherAnswerUI.AnswerElement);
    }

    protected override SurveyAnswerUIBase AddAnswerUI(bool isOther = false) {
        var answerUI = base.AddAnswerUI(isOther);
        RecalculateAnswerIndices();
        return answerUI;
    }

    /// <summary>Recalculates indices after changes.</summary>
    protected virtual void RecalculateAnswerIndices() {
        int count = _addedAnswers.Count;
        for (int i = 0; i < count; i++) {
            _addedAnswers[i].UpdateIndex(i);
            if (_addedAnswers[i] is SurveyAnswerUIEditor answerEditor) {
                bool canMoveUp = i > 0;
                bool canMoveDown = i < count - 1;
                answerEditor.SetMoveButtonsEnabled(canMoveUp, canMoveDown);
            }
        }

        if (_otherAnswerUI != null) {
            _otherAnswerUI.UpdateIndex(_addedAnswers.Count);
            if (_otherAnswerUI is SurveyAnswerUIEditor otherEditor) {
                otherEditor.SetMoveButtonsEnabled(false, false);
            }
        }
    }

    #endregion

    #region Modal Handling

    //protected virtual void OnEditQuestionClicked(ClickEvent evt) {
    //    var modal = _root.Q<VisualElement>("edit-question-modal");
    //    if (modal == null) return;

    //    bool open = modal.style.display != DisplayStyle.Flex;

    //    CloseCurrentModal();

    //    if (open)
    //        ShowModal(modal);
    //    else
    //        HideQuestionModal();

    //    evt.StopPropagation();
    //}

    /// <summary>Displays modal near button.</summary>
    //protected virtual void ShowModal(VisualElement modal) {
    //    modal.style.display = DisplayStyle.Flex;
    //    _currentlyOpenModal = modal;

    //    RegisterQuestionModalButtonEvents(modal);
    //    RegisterOutsideClickHandler(modal);
    //}

    //public virtual void CloseCurrentModal() {
    //    foreach (var a in _addedAnswers)
    //        a.HideCurrentModal();

    //    _otherAnswerUI?.HideCurrentModal();
    //    HideQuestionModal();
    //}

    //protected virtual void HideQuestionModal() {
    //    if (_currentlyOpenModal == null) return;

    //    _currentlyOpenModal.style.display = DisplayStyle.None;
    //    UnregisterOutsideClickHandler();

    //    _currentlyOpenModal = null;
    //}

    public virtual void SetQuestionMoveButtonsEnabled(bool canMoveUp, bool canMoveDown) {
        var modal = _root.Q<VisualElement>("edit-question-modal");
        var moveUpButton = modal?.Q<Button>("move-up-button") ?? _root.Q<Button>("move-up-button");
        var moveDownButton = modal?.Q<Button>("move-down-button") ?? _root.Q<Button>("move-down-button");

        moveUpButton?.SetEnabled(canMoveUp);
        moveDownButton?.SetEnabled(canMoveDown);
    }

    #region Modal Events

    protected virtual void RegisterQuestionModalButtonEvents() {
        var modal = _root.Q<VisualElement>("edit-question-modal");
        var moveUpButton = _root.Q<Button>("move-up-button") ?? modal?.Q<Button>("move-up-button");
        var moveDownButton = _root.Q<Button>("move-down-button") ?? modal?.Q<Button>("move-down-button");
        var deleteButton = _root.Q<Button>("delete-question-button") ?? _root.Q<Button>("delete-option-button") ?? modal?.Q<Button>("delete-option-button");
        var imageButton = _root.Q<Button>("image-button");

        if (moveUpButton == null || moveDownButton == null || deleteButton == null || imageButton == null) {
            Debug.LogWarning($"[{QuestionID}] Button check: moveUp={moveUpButton != null}, moveDown={moveDownButton != null}, delete={deleteButton != null}, image={imageButton != null}");
        }

        // Remove old callbacks first
        if (_onMoveUp != null && moveUpButton != null) moveUpButton.clicked -= _onMoveUp;
        if (_onMoveDown != null && moveDownButton != null) moveDownButton.clicked -= _onMoveDown;
        if (_onDelete != null && deleteButton != null) deleteButton.clicked -= _onDelete;
        if (_onUpload != null && imageButton != null) imageButton.clicked -= _onUpload;

        // Create new ones with dynamic index calculation
        _onMoveUp = () => {
            int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(this);
            OnQuestionMoved?.Invoke(dynamicIdx, -1);
        };

        _onMoveDown = () => {
            int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(this);
            OnQuestionMoved?.Invoke(dynamicIdx, 1);
        };

        _onDelete = () => {
            int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(this);
            OnQuestionDeleted?.Invoke(dynamicIdx);
        };

        _onUpload = () => {
            int dynamicIdx = _surveyUIBuilder.GetQuestionIndex(this);
            OnUploadImage?.Invoke(dynamicIdx);
        };

        // Register
        if (moveUpButton != null) moveUpButton.clicked += _onMoveUp;
        if (moveDownButton != null) moveDownButton.clicked += _onMoveDown;
        if (deleteButton != null) deleteButton.clicked += _onDelete;
        if (imageButton != null) imageButton.clicked += _onUpload;
    }

    #endregion

    #endregion

    #region Outside Click Handling

    //protected virtual void RegisterOutsideClickHandler(VisualElement modal) {
    //    GetRoot(modal).RegisterCallback<PointerDownEvent>(OnOutsideClick, TrickleDown.TrickleDown);
    //}

    //protected virtual void UnregisterOutsideClickHandler() {
    //    if (_currentlyOpenModal == null) return;

    //    GetRoot(_currentlyOpenModal)
    //        .UnregisterCallback<PointerDownEvent>(OnOutsideClick, TrickleDown.TrickleDown);
    //}

    //protected virtual void OnOutsideClick(PointerDownEvent evt) {
    //    if (_currentlyOpenModal == null) return;

    //    if (!_currentlyOpenModal.ContainsPoint(_currentlyOpenModal.WorldToLocal(evt.position)))
    //        HideQuestionModal();
    //}

    protected virtual VisualElement GetRoot(VisualElement element) {
        while (element.parent != null)
            element = element.parent;

        return element;
    }

    #endregion

    #region Dropdown

    protected virtual void PopulateCameraViewDropdown(DropdownField dropdown) {
        if (dropdown == null) return;

        var choices = new List<string>();

        choices.Add("Žádný");

        foreach (var vp in _viewPoints)
            choices.Add(vp.Name);

        dropdown.choices = choices;

        if (choices.Count > 0) {
            dropdown.value = choices[0];
            OnViewpointSelected?.Invoke(QuestionID, "" /*_viewPoints[0].ID*/);
        }
    }

    protected virtual void SetViewPointRender(string viewPointId) {
        var cameraView = _root.Q<VisualElement>("camera-view");
        cameraView.style.display = DisplayStyle.Flex;
        cameraView.style.backgroundImage = Background.FromRenderTexture(
            _surveyUIBuilder.CreateRenderTexture(viewPointId)
        );
    }

    #endregion

}
