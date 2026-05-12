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

    #endregion

    #region Events

    #region Public events

    public event Action<int, string> OnTitleChanged;
    public event Action<int, string> OnDescriptionChanged;
    public event Action<int> OnQuestionDeleted;
    public event Action<int, int> OnQuestionMoved;
    public event Action<int, string> OnViewpointSelected;
    public event Action<int> OnUploadImage;

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
    public override void AddAnswer(string answerText, bool isOther = false) {
        if (_optionsList == null || _answerTemplate == null) {
            Debug.LogWarning("Missing options list or template!");
            return;
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

        if (!isOther) {
            TextField tf = FindTextFieldRecursive(element);
            if (tf != null)
                tf.value = answerText;
        }

        var answerUI = CreateAnswerUI(element, index, isOther);

        if (isOther) {
            var button = element.Q<CustomRadioButton>();
            if (button != null) {
                element.Q<CustomRadioButton>().Placeholder = "Other";
            }
            _otherAnswerUI = answerUI;
        } else {
            _addedAnswers.Add(answerUI);
        }
    }

    public void SetSelectedView(ViewPoint viewPoint) {
        var dropdown = _root.Q<DropdownField>("camera-view-dropdown");
        int index = -1; // choices zaèínají na "žádný", a to není vp
        foreach (string choice in dropdown.choices) {
            if (choice == viewPoint.Name) {
                dropdown.value = choice;
                SetViewPointRender(_viewPoints[index].ID); // this errors during deserialize - out of range
                return;
            }
            index++;
        }
    }

    public Tuple<int, string> GetSelectedViewName() {
        var dropdown = _root.Q<DropdownField>("camera-view-dropdown");
        return new Tuple<int, string>(dropdown.index, dropdown.value);
    }

    #endregion

    #region UI Input Registration

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
                _root.Q<VisualElement>("camera-view").style.backgroundImage = null;
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
    }

    public virtual void MoveAnswerDown(int index) {
        if (index < 0 || index >= _addedAnswers.Count - 1) return;
        SwapAnswers(index, index + 1);
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

    /// <summary>Recalculates indices after changes.</summary>
    protected virtual void RecalculateAnswerIndices() {
        for (int i = 0; i < _addedAnswers.Count; i++)
            _addedAnswers[i].UpdateIndex(i);

        _otherAnswerUI?.UpdateIndex(_addedAnswers.Count);
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

    #region Modal Events

    protected virtual void RegisterQuestionModalButtonEvents() {
        var moveUpButton = _root.Q<Button>("move-up-button");
        var moveDownButton = _root.Q<Button>("move-down-button");
        var deleteButton = _root.Q<Button>("delete-option-button");
        var imageButton = _root.Q<Button>("image-button");

        int index = _surveyUIBuilder.GetQuestionIndex(this);

        if (moveUpButton == null || moveDownButton == null || deleteButton == null  || imageButton == null) {
            Debug.LogError($"[{QuestionID}] Failed to register button events: One or more buttons not found in UIDocument.");
            return;
        }

        // Remove old callbacks first
        if (_onMoveUp != null) moveUpButton.clicked -= _onMoveUp;
        if (_onMoveDown != null) moveDownButton.clicked -= _onMoveDown;
        if (_onDelete != null) deleteButton.clicked -= _onDelete;
        if (_onUpload != null) imageButton.clicked -= _onUpload;

        // Create new ones
        _onMoveUp = () => {
            OnQuestionMoved?.Invoke(index, -1);
        };

        _onMoveDown = () => {
            OnQuestionMoved?.Invoke(index, 1);
        };

        _onDelete = () => {
            OnQuestionDeleted?.Invoke(index);
        };

        _onUpload = () => {
            OnUploadImage?.Invoke(index);
        };

        // Register
        moveUpButton.clicked += _onMoveUp;
        moveDownButton.clicked += _onMoveDown;
        deleteButton.clicked += _onDelete;
        imageButton.clicked += _onUpload;

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