using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;
using System.Collections.Generic;
using System;

public class SurveyQuestionUI : ISurveyQuestionBuilderUI {

    #region Fields & Properties

    public int QuestionID { get; }

    private VisualElement _root;
    private SurveyUIBuilder _surveyUIBuilder;
    private QuestionType _questionType;
    private List<SerializableViewPoint> _viewPoints;

    private List<SurveyAnswerUI> _addedAnswers = new();
    private SurveyAnswerUI _otherAnswerUI;

    private VisualElement _optionsList;
    private VisualTreeAsset _answerTemplate;

    // Modal tracking
    private VisualElement _currentlyOpenModal;
    private VisualElement _originalParent;
    private int _originalIndex = -1;

    public VisualElement QuestionElement => _root;

    #endregion

    #region Events

    #region Public events

    public event Action<int, string> OnTitleChanged;
    public event Action<int, string> OnDescriptionChanged;
    public event Action<int> OnQuestionDeleted;
    public event Action<int, int> OnQuestionMoved;
    public event Action<int, SurveyAnswerUI> OnAnswerAdded;
    public event Action<int> OnAnswerOtherAdded;
    public event Action<int> OnAnswerRemoved;
    public event Action<int, string> OnViewpointSelected;
    public event Action<int, int, string> OnAnswerTextChanged;

    #endregion

    #region Internal events

    private Action _onMoveUp;
    private Action _onMoveDown;
    private Action _onDelete;

    #endregion

    #endregion

    public SurveyQuestionUI(
        VisualElement root,
        int questionId,
        QuestionType questionType,
        List<SerializableViewPoint> viewPoints,
        SurveyUIBuilder uiBuilder,
        bool isDeserialized = false) {

        _root = root;
        QuestionID = questionId;
        _questionType = questionType;
        _viewPoints = viewPoints;
        _surveyUIBuilder = uiBuilder;

        LoadAnswerTemplate();
        InitializeOptionsList();

        RegisterInputs();
    }


    #region Initialization

    /// <summary>Loads the correct answer template based on question type.</summary>
    private void LoadAnswerTemplate() {
        var mapping = _surveyUIBuilder.questionUIMapping;

        if (mapping == null) {
            Debug.LogError("QuestionUIMapping not found!");
            return;
        }

        _answerTemplate = mapping.GetAnswerUITemplate(_questionType);

        if (_answerTemplate == null)
            Debug.LogWarning($"No template for {_questionType}");
    }

    /// <summary>Finds and prepares the options container.</summary>
    private void InitializeOptionsList() {
        if (_root == null) return;

        _optionsList = _root.Q<RadioButtonGroup>("options-list") ??
                       _root.Q<VisualElement>("options-list");

        _optionsList?.Clear();
    }

    #endregion

    #region Interface for editing the question

    public void SetTitle(string title) {
        _root.Q<TextField>("question-title").value = title;
    }

    public void SetDescription(string desc) {
        _root.Q<TextField>("question-description").value = desc;
    }

    // Make it virtual or abstract - check the other add answer function and how different it is (this is for code calls, theo ther is for ui calls)
    public void AddAnswer(string answerText, bool isOther = false) {
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

        var answerUI = new SurveyAnswerUI(element, index, this, isOther);

        if (isOther) {
            element.Q<CustomRadioButton>().Placeholder = "Other";
            _otherAnswerUI = answerUI;
        } else {
            _addedAnswers.Add(answerUI);
        }
    }

    #endregion

    #region UI Input Registration

    /// <summary>Registers all UI callbacks.</summary>
    private void RegisterInputs() {
        RegisterTextInputs();
        RegisterButtons();
        RegisterDropdown();
    }

    private void RegisterTextInputs() {
        _root.Q<TextField>("question-title")?.RegisterValueChangedCallback(
            evt => OnTitleChanged?.Invoke(QuestionID, evt.newValue));

        _root.Q<TextField>("question-description")?.RegisterValueChangedCallback(
            evt => OnDescriptionChanged?.Invoke(QuestionID, evt.newValue));
    }

    private void RegisterButtons() {
        var addOptionButton = _root.Q<Button>("add-option-button");
        if (addOptionButton != null) {
            addOptionButton.clicked += () =>
                OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
        } else {
            Debug.LogWarning("[RegisterButtons] add-option-button not found");
        }

        var addOtherButton = _root.Q<Button>("add-other-option-button");
        if (addOtherButton != null) {
            addOtherButton.clicked += () => {
                if (_otherAnswerUI == null) {
                    OnAnswerOtherAdded?.Invoke(QuestionID);
                    AddAnswerUI(true);
                }
            };
        } else {
            Debug.LogWarning("[RegisterButtons] add-other-option-button not found");
        }

        var editButton = _root.Q<VisualElement>("edit-question-button");
        if (editButton != null) {
            editButton.RegisterCallback<ClickEvent>(OnEditQuestionClicked);
        } else {
            Debug.LogWarning("[RegisterButtons] edit-question-button not found");
        }
    }

    private void RegisterDropdown() {
        var dropdown = _root.Q<DropdownField>("camera-view-dropdown");
        PopulateCameraViewDropdown(dropdown);

        dropdown?.RegisterValueChangedCallback(evt => {
            int index = dropdown.index;
            if (index >= 0 && index < _viewPoints.Count)
                OnViewpointSelected?.Invoke(QuestionID, _viewPoints[index].ID);
        });
    }

    #endregion

    #region Answer Management

    public void AddInitialAnswer() {
        OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI());
    }

    /// <summary>Adds an answer UI element.</summary>
    private SurveyAnswerUI AddAnswerUI(bool isOther = false) {
        Debug.Log($"[AddAnswerUI] START | isOther: {isOther}");

        if (_optionsList == null) {
            Debug.LogError("[AddAnswerUI] _optionsList is NULL");
            return null;
        }

        if (_answerTemplate == null) {
            Debug.LogError("[AddAnswerUI] _answerTemplate is NULL");
            return null;
        }

        var element = _answerTemplate.Instantiate();
        Debug.Log("[AddAnswerUI] Template instantiated");

        int index = _addedAnswers.Count;
        Debug.Log($"[AddAnswerUI] Current index: {index}, existing answers count: {_addedAnswers.Count}");

        if (isOther) {
            Debug.Log("[AddAnswerUI] Handling OTHER answer");

            _optionsList.Add(element);
            Debug.Log("[AddAnswerUI] Element added to _optionsList");

            var radio = element.Q<CustomRadioButton>();
            if (radio == null) {
                Debug.LogError("[AddAnswerUI] CustomRadioButton NOT FOUND in template");
            } else {
                radio.Placeholder = "Other";
                Debug.Log("[AddAnswerUI] Set placeholder to 'Other'");
            }

            _otherAnswerUI = new SurveyAnswerUI(element, index, this, true);
            Debug.Log("[AddAnswerUI] Created _otherAnswerUI");

            return _otherAnswerUI;
        }

        Debug.Log("[AddAnswerUI] Handling NORMAL answer");

        InsertAnswerElement(element);
        Debug.Log("[AddAnswerUI] Element inserted via InsertAnswerElement");

        var answerUI = new SurveyAnswerUI(element, index, this, false);
        Debug.Log("[AddAnswerUI] SurveyAnswerUI created");

        _addedAnswers.Add(answerUI);
        Debug.Log($"[AddAnswerUI] Added to _addedAnswers. New count: {_addedAnswers.Count}");

        Debug.Log("[AddAnswerUI] END");

        return answerUI;
    }

    /// <summary>Inserts answer before "Other" if it exists.</summary>
    private void InsertAnswerElement(VisualElement element) {
        if (_otherAnswerUI != null) {
            int idx = _optionsList.IndexOf(_otherAnswerUI.AnswerElement);
            _optionsList.Insert(idx, element);
        } else {
            _optionsList.Add(element);
        }
    }

    /// <summary>Deletes an answer.</summary>
    public void DeleteAnswer(int index) {
        if (index < 0) return;

        if (_otherAnswerUI != null && index == _otherAnswerUI.AnswerIndex) {
            _optionsList.Remove(_otherAnswerUI.AnswerElement);
            _otherAnswerUI = null;
            OnAnswerRemoved?.Invoke(index);
            return;
        }

        if (index >= _addedAnswers.Count) return;

        var answer = _addedAnswers[index];

        _optionsList.Remove(answer.AnswerElement);
        _addedAnswers.RemoveAt(index);

        RecalculateAnswerIndices();

        OnAnswerRemoved?.Invoke(index);
    }

    #endregion

    #region Answer Reordering

    public void MoveAnswerUp(int index) {
        if (index <= 0 || index >= _addedAnswers.Count) return;
        SwapAnswers(index, index - 1);
    }

    public void MoveAnswerDown(int index) {
        if (index < 0 || index >= _addedAnswers.Count - 1) return;
        SwapAnswers(index, index + 1);
    }

    private void SwapAnswers(int a, int b) {
        (_addedAnswers[a], _addedAnswers[b]) = (_addedAnswers[b], _addedAnswers[a]);

        RefreshAnswerOrder();
        RecalculateAnswerIndices();
    }

    private void RefreshAnswerOrder() {
        _optionsList.Clear();

        foreach (var a in _addedAnswers)
            _optionsList.Add(a.AnswerElement);

        if (_otherAnswerUI != null)
            _optionsList.Add(_otherAnswerUI.AnswerElement);
    }

    /// <summary>Recalculates indices after changes.</summary>
    private void RecalculateAnswerIndices() {
        for (int i = 0; i < _addedAnswers.Count; i++)
            _addedAnswers[i].UpdateIndex(i);

        _otherAnswerUI?.UpdateIndex(_addedAnswers.Count);
    }

    #endregion

    #region Modal Handling

    private void OnEditQuestionClicked(ClickEvent evt) {
        var modal = _root.Q<VisualElement>("edit-question-modal");
        if (modal == null) return;

        bool open = modal.style.display != DisplayStyle.Flex;

        CloseCurrentModal();

        if (open)
            ShowModal(modal);
        else
            HideQuestionModal();

        evt.StopPropagation();
    }

    /// <summary>Displays modal near button.</summary>
    private void ShowModal(VisualElement modal) {
        modal.style.display = DisplayStyle.Flex;
        _currentlyOpenModal = modal;

        RegisterQuestionModalButtonEvents(modal);
        RegisterOutsideClickHandler(modal);
    }

    public void CloseCurrentModal() {
        foreach (var a in _addedAnswers)
            a.HideCurrentModal();

        _otherAnswerUI?.HideCurrentModal();
        HideQuestionModal();
    }

    private void HideQuestionModal() {
        if (_currentlyOpenModal == null) return;

        _currentlyOpenModal.style.display = DisplayStyle.None;
        UnregisterOutsideClickHandler();

        _currentlyOpenModal = null;
    }

    #region Modal Events

    private void RegisterQuestionModalButtonEvents(VisualElement modal) {
        var moveUpButton = modal.Q<Button>("move-up-button");
        var moveDownButton = modal.Q<Button>("move-down-button");
        var deleteButton = modal.Q<Button>("delete-option-button");

        int index = _surveyUIBuilder.GetQuestionIndex(this);

        // Remove old callbacks first
        if (_onMoveUp != null) moveUpButton.clicked -= _onMoveUp;
        if (_onMoveDown != null) moveDownButton.clicked -= _onMoveDown;
        if (_onDelete != null) deleteButton.clicked -= _onDelete;

        // Create new ones
        _onMoveUp = () => {
            OnQuestionMoved?.Invoke(index, -1);
            HideQuestionModal();
        };

        _onMoveDown = () => {
            OnQuestionMoved?.Invoke(index, 1);
            HideQuestionModal();
        };

        _onDelete = () => {
            OnQuestionDeleted?.Invoke(index);
            HideQuestionModal();
        };

        // Register
        moveUpButton.clicked += _onMoveUp;
        moveDownButton.clicked += _onMoveDown;
        deleteButton.clicked += _onDelete;
    }

    #endregion

    #endregion

    #region Outside Click Handling

    private void RegisterOutsideClickHandler(VisualElement modal) {
        GetRoot(modal).RegisterCallback<PointerDownEvent>(OnOutsideClick, TrickleDown.TrickleDown);
    }

    private void UnregisterOutsideClickHandler() {
        if (_currentlyOpenModal == null) return;

        GetRoot(_currentlyOpenModal)
            .UnregisterCallback<PointerDownEvent>(OnOutsideClick, TrickleDown.TrickleDown);
    }

    private void OnOutsideClick(PointerDownEvent evt) {
        if (_currentlyOpenModal == null) return;

        if (!_currentlyOpenModal.ContainsPoint(_currentlyOpenModal.WorldToLocal(evt.position)))
            HideQuestionModal();
    }

    private VisualElement GetRoot(VisualElement element) {
        while (element.parent != null)
            element = element.parent;

        return element;
    }

    #endregion

    #region Dropdown

    private void PopulateCameraViewDropdown(DropdownField dropdown) {
        if (dropdown == null) return;

        var choices = new List<string>();
        foreach (var vp in _viewPoints)
            choices.Add(vp.Name);

        dropdown.choices = choices;

        if (choices.Count > 0) {
            dropdown.value = choices[0];
            OnViewpointSelected?.Invoke(QuestionID, _viewPoints[0].ID);
        }
    }

    #endregion

    #region Helpers

    private TextField FindTextFieldRecursive(VisualElement root) {
        if (root is TextField tf) return tf;

        foreach (var child in root.Children()) {
            var result = FindTextFieldRecursive(child);
            if (result != null) return result;
        }

        return null;
    }

    #endregion
}