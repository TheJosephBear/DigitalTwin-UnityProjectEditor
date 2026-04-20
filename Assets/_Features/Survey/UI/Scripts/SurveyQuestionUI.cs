using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;
using System.Collections.Generic;
using System;

// Before refactor
/*
public class SurveyQuestionUIaaa : ISurveyQuestionBuilderUI {
    public int QuestionID { get; }
    private VisualElement _root;
    private SurveyUIBuilder _surveyUIBuilder;
    private QuestionType _questionType;
    private List<SerializableViewPoint> _viewPoints;

    // Track added answers - regular answers and "Other" answer are tracked separately
    private List<SurveyAnswerUI> _addedAnswers = new List<SurveyAnswerUI>();
    private SurveyAnswerUI _otherAnswerUI; // Tracks the special "Other" answer that always appears last
    private VisualElement _optionsList;

    // Reference to the answer template
    private VisualTreeAsset _answerTemplate;

    // Track currently open question modal
    private VisualElement _currentlyOpenModal = null;
    private VisualElement _originalParent = null;
    private int _originalIndex = -1;

    /// <summary>The root visual element for this question (may be null if template was missing).</summary>
    public VisualElement QuestionElement => _root;


    #region Events

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

    public SurveyQuestionUI(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder, bool isDeserialized = false) {
        _root = root;
        _surveyUIBuilder = uiBuilder;
        QuestionID = questionId;
        _questionType = questionType;
        _viewPoints = viewPoints;

        // Get the answer template from QuestionUIMapping
        QuestionUIMapping mapping = _surveyUIBuilder.questionUIMapping;
        if (mapping != null) {
            _answerTemplate = mapping.GetAnswerUITemplate(_questionType);
            if (_answerTemplate == null) {
                Debug.LogWarning($"No answer template found for question type: {_questionType}");
            }
        } else {
            Debug.LogError("QuestionUIMapping not found in scene!");
        }

        if (_root == null) return;

        // Get the options list container
        _optionsList = _root.Q<RadioButtonGroup>("options-list");
        if (_optionsList == null) {
            // Try alternate container names if RadioButtonGroup isn't found
            _optionsList = _root.Q<VisualElement>("options-list");
        }
        
        _optionsList.Clear(); // Clear any existing options in the UI
        if(!isDeserialized) AddAnswerUI(); // Add the first answer UI element by default

        RegisterInputs();
    }

    private void RegisterInputs() {
        var questionTitleField = _root.Q<TextField>("question-title");
        var questionDescriptionField = _root.Q<TextField>("question-description");
        var addOptionButton = _root.Q<Button>("add-option-button");
        var addOptionOtherButton = _root.Q<Button>("add-other-option-button");
        var cameraViewDropdown = _root.Q<DropdownField>("camera-view-dropdown");
        var editQuestionButton = _root.Q<VisualElement>("edit-question-button");

        if (questionTitleField != null) {
            questionTitleField.RegisterValueChangedCallback(evt => {
                OnTitleChanged?.Invoke(QuestionID, evt.newValue);
            });
        }

        if (questionDescriptionField != null) {
            questionDescriptionField.RegisterValueChangedCallback(evt => {
                OnDescriptionChanged?.Invoke(QuestionID, evt.newValue);
            });
        }

        if (addOptionButton != null) {
            addOptionButton.clicked += () => {
                OnAnswerAdded?.Invoke(QuestionID, AddAnswerUI(false));
            };
        }

        if (addOptionOtherButton != null) {
            addOptionOtherButton.clicked += () => {
                // Only allow one "Other" answer per question
                if (_otherAnswerUI == null) {
                    OnAnswerOtherAdded?.Invoke(QuestionID);
                    AddAnswerUI(true);
                }
            };
        }

        if (editQuestionButton != null) {
            editQuestionButton.RegisterCallback<ClickEvent>(OnEditQuestionButtonClicked);
        }

        if (cameraViewDropdown != null) {
            cameraViewDropdown.RegisterValueChangedCallback(evt => {
                // Handle camera view change based on selected value
                int index = cameraViewDropdown.index;

                if (index >= 0 && index < _viewPoints.Count) {
                    OnViewpointSelected?.Invoke(QuestionID, _viewPoints[index].ID);
                }
            });
        }

        PopulateCameraViewDropdown(cameraViewDropdown);
    }

    public void SetTitle(string title) {
        _root.Q<TextField>("question-title").value = title;
    }

    public void SetDescription(string desc) {
        _root.Q<TextField>("question-description").value = desc;
    }

    public void AddAnswer(string answerText, bool isOther=false) {
        if (_optionsList == null) {
            Debug.LogWarning("Options list container not found!");
            return;
        }

        if (_answerTemplate == null) {
            Debug.LogWarning("Answer template not set!");
            return;
        }

        // Instantiate the answer template
        TemplateContainer answerElement = _answerTemplate.Instantiate();

        int answerIndex;
        if (isOther) {
            // "Other" answer: add to the end and track separately
            answerIndex = _addedAnswers.Count;
            _optionsList.Add(answerElement);

            answerElement.Q<CustomRadioButton>().Placeholder = "Jiné"; // Set label to "Other"
            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, this);
            _otherAnswerUI = answerUI;
        } else {
            // Regular answer: insert before "Other" answer if it exists, otherwise add to end
            answerIndex = _addedAnswers.Count;

            if (_otherAnswerUI != null) {
                // Insert before the "Other" answer
                int insertIndex = _optionsList.IndexOf(_otherAnswerUI.AnswerElement);
                _optionsList.Insert(insertIndex, answerElement);
            } else {
                // No "Other" answer exists, add to end
                _optionsList.Add(answerElement);
            }

            TextField textField = FindTextFieldRecursive(answerElement);

            if (textField != null) {
                textField.value = answerText;
            } else {
                Debug.LogWarning("No TextField found in answer template!");
            }

            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, this, isOther);
            _addedAnswers.Add(answerUI);
        }

    }

    private void OnEditQuestionButtonClicked(ClickEvent evt) {
        var editQuestionButton = _root.Q<VisualElement>("edit-question-button");
        var editContainer = editQuestionButton.parent.Q<VisualElement>("edit-question-modal");

        if (editContainer != null) {
            bool isCurrentlyHidden = editContainer.style.display != DisplayStyle.Flex;

            CloseCurrentModal();

            if (!isCurrentlyHidden) {
                editContainer.style.display = DisplayStyle.None;
            }

            if (isCurrentlyHidden) {
                editContainer.parent.style.overflow = Overflow.Visible;

                // Find survey-scroll-view or a suitable ancestor to host the modal
                VisualElement current = editContainer.parent;
                int maxParentsToCheck = 10;
                VisualElement hostContainer = null;

                while (current != null && maxParentsToCheck > 0) {
                    if (current.name == "survey-scroll-view" || current.name == "question-container") {
                        hostContainer = current;
                        current.style.overflow = Overflow.Visible;
                        break;
                    }
                    current = current.parent;
                    maxParentsToCheck--;
                }

                if (hostContainer != null) {
                    editContainer.style.display = DisplayStyle.Flex;

                    _originalParent = editContainer.parent;
                    _originalIndex = _originalParent.IndexOf(editContainer);

                    Rect buttonBound = editQuestionButton.worldBound;
                    Rect hostBound = hostContainer.worldBound;

                    editContainer.RemoveFromHierarchy();
                    hostContainer.Add(editContainer);

                    float buttonRightEdge = buttonBound.x + buttonBound.width;
                    float rightDistance = hostBound.width - (buttonRightEdge - hostBound.x);
                    float topPosition = buttonBound.y + buttonBound.height - hostBound.y;

                    editContainer.style.position = Position.Absolute;
                    editContainer.style.right = new StyleLength(new Length(rightDistance, LengthUnit.Pixel));
                    editContainer.style.top = new StyleLength(new Length(topPosition, LengthUnit.Pixel));
                    editContainer.style.left = StyleKeyword.Auto;
                    editContainer.style.bottom = StyleKeyword.Auto;

                    editContainer.BringToFront();
                } else {
                    editContainer.style.display = DisplayStyle.Flex;
                }

                _currentlyOpenModal = editContainer;
                RegisterQuestionModalButtonEvents(editContainer);
                RegisterQuestionOutsideClickHandler(editContainer);
            } else {
                _currentlyOpenModal = null;
                UnregisterQuestionOutsideClickHandler();
            }
        }

        evt.StopPropagation();
    }

    private void PopulateCameraViewDropdown(DropdownField dropdown) {
        if (dropdown == null) return;

        List<string> choiceLabels = new List<string>();

        foreach (SerializableViewPoint viewPoint in _viewPoints) {
            choiceLabels.Add(viewPoint.Name);
        }

        dropdown.choices = choiceLabels;

        if (dropdown.choices.Count > 0) {
            dropdown.value = dropdown.choices[0];
            OnViewpointSelected?.Invoke(QuestionID, _viewPoints[0].ID);
        }
    }

    private SurveyAnswerUI AddAnswerUI(bool isOther = false) {
        if (_optionsList == null) {
            Debug.LogWarning("Options list container not found!");
            return null;
        }

        if (_answerTemplate == null) {
            Debug.LogWarning("Answer template not set!");
            return null;
        }

        // Instantiate the answer template
        TemplateContainer answerElement = _answerTemplate.Instantiate();

        int answerIndex;
        if (isOther) {
            // "Other" answer: add to the end and track separately
            answerIndex = _addedAnswers.Count;
            _optionsList.Add(answerElement);

            answerElement.Q<CustomRadioButton>().Placeholder = "Jiné"; // Set label to "Other"
            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, this);
            _otherAnswerUI = answerUI;

            return answerUI;
        } else {
            // Regular answer: insert before "Other" answer if it exists, otherwise add to end
            answerIndex = _addedAnswers.Count;

            if (_otherAnswerUI != null) {
                // Insert before the "Other" answer
                int insertIndex = _optionsList.IndexOf(_otherAnswerUI.AnswerElement);
                _optionsList.Insert(insertIndex, answerElement);
            } else {
                // No "Other" answer exists, add to end
                _optionsList.Add(answerElement);
            }

            // Create SurveyAnswerUI instance to manage this answer
            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, this, isOther);
            _addedAnswers.Add(answerUI);

            return answerUI;
        }
    }

    public void CloseCurrentModal() {
        // Close modals in all regular answers
        foreach (var answer in _addedAnswers) {
            answer.HideCurrentModal();
        }

        // Close modal for "Other" answer if it exists
        if (_otherAnswerUI != null) {
            _otherAnswerUI.HideCurrentModal();
        }

        HideQuestionModal();
    }

    private void HideQuestionModal() {
        if (_currentlyOpenModal != null) {
            _currentlyOpenModal.style.display = DisplayStyle.None;

            if (_originalParent != null) {
                _currentlyOpenModal.RemoveFromHierarchy();
                _originalParent.Insert(_originalIndex, _currentlyOpenModal);
                _originalParent = null;
                _originalIndex = -1;
            }

            UnregisterQuestionOutsideClickHandler();
            _currentlyOpenModal = null;
        }
    }

    private void OnRootPointerDown(PointerDownEvent evt) {
        if (_currentlyOpenModal != null && _currentlyOpenModal.style.display == DisplayStyle.Flex) {
            if (!_currentlyOpenModal.ContainsPoint(_currentlyOpenModal.WorldToLocal(evt.position))) {
                HideQuestionModal();
            }
        }
    }

    private void RegisterQuestionOutsideClickHandler(VisualElement modal) {
        VisualElement root = modal;
        while (root.parent != null) {
            root = root.parent;
        }
        root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    private void UnregisterQuestionOutsideClickHandler() {
        if (_currentlyOpenModal != null) {
            VisualElement root = _currentlyOpenModal;
            while (root.parent != null) {
                root = root.parent;
            }
            root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }

    private void RegisterQuestionModalButtonEvents(VisualElement editContainer) {
        var moveUpButton = editContainer.Q<Button>("move-up-button");
        var moveDownButton = editContainer.Q<Button>("move-down-button");
        var deleteButton = editContainer.Q<Button>("delete-option-button");

        editContainer.Q<Button>("move-up-button").clicked += () => { };

        if (deleteButton != null) {
            deleteButton.clicked += () => {
                if (_surveyUIBuilder != null) {
                    int index = _surveyUIBuilder.GetQuestionIndex(this);
                    if (index >= 0) {
                        HideQuestionModal();
                        OnQuestionDeleted.Invoke(index);
                    }
                }
            };
        }

        if (moveUpButton != null) {
            moveUpButton.clicked += () => {
                if (_surveyUIBuilder != null) {
                    int index = _surveyUIBuilder.GetQuestionIndex(this);
                    if (index >= 0) {
                        OnQuestionMoved.Invoke(index, -1);
                    }
                }
                HideQuestionModal();
            };
        }

        if (moveDownButton != null) {
            moveDownButton.clicked += () => {
                if (_surveyUIBuilder != null) {
                    int index = _surveyUIBuilder.GetQuestionIndex(this);
                    if (index >= 0) {
                        OnQuestionMoved.Invoke(index, 1);
                    }
                }
                HideQuestionModal();
            };
        }
    }

    public void DeleteAnswer(int answerIndex) {
        if (answerIndex < 0) return;

        // Check if deleting the "Other" answer
        if (_otherAnswerUI != null && answerIndex == _otherAnswerUI.AnswerIndex) {
            var answer = new AnswerBase { Idx = answerIndex };
            OnAnswerRemoved.Invoke(answerIndex);

            // Remove "Other" answer from UI
            _optionsList.Remove(_otherAnswerUI.AnswerElement);
            _otherAnswerUI = null;
            return;
        }

        if (answerIndex >= _addedAnswers.Count) return;

        var answerUI = _addedAnswers[answerIndex];

        // Create answer object for removal from data model
        var answerToRemove = new AnswerBase { Idx = answerIndex };
        OnAnswerRemoved.Invoke(answerIndex);

        // Remove from UI
        _optionsList.Remove(answerUI.AnswerElement);
        _addedAnswers.RemoveAt(answerIndex);

        // Update indices for all remaining regular answers
        for (int i = answerIndex; i < _addedAnswers.Count; i++) {
            _addedAnswers[i].UpdateIndex(i);
        }

        // Update "Other" answer index if it exists
        if (_otherAnswerUI != null) {
            _otherAnswerUI.UpdateIndex(_addedAnswers.Count);
        }
    }

    public void MoveAnswerUp(int answerIndex) {
        if (answerIndex <= 0 || answerIndex >= _addedAnswers.Count) return;
        // Prevent moving the "Other" answer
        if (_otherAnswerUI != null && answerIndex == _otherAnswerUI.AnswerIndex) return;

        SwapAnswers(answerIndex, answerIndex - 1);
    }

    public void MoveAnswerDown(int answerIndex) {
        if (answerIndex < 0 || answerIndex >= _addedAnswers.Count - 1) return;
        // Prevent moving the "Other" answer
        if (_otherAnswerUI != null && answerIndex == _otherAnswerUI.AnswerIndex) return;

        SwapAnswers(answerIndex, answerIndex + 1);
    }

    private void SwapAnswers(int index1, int index2) {
        // Swap in the list
        var temp = _addedAnswers[index1];
        _addedAnswers[index1] = _addedAnswers[index2];
        _addedAnswers[index2] = temp;

        // Reorder in the visual container
        _optionsList.Clear();
        foreach (var answerUI in _addedAnswers) {
            _optionsList.Add(answerUI.AnswerElement);
        }

        // Ensure "Other" answer always appears last
        if (_otherAnswerUI != null) {
            _optionsList.Add(_otherAnswerUI.AnswerElement);
        }

        // Update indices for all regular answers
        for (int i = 0; i < _addedAnswers.Count; i++) {
            _addedAnswers[i].UpdateIndex(i);
        }

        // Update "Other" answer index if it exists
        if (_otherAnswerUI != null) {
            _otherAnswerUI.UpdateIndex(_addedAnswers.Count);
        }
    }

    private TextField FindTextFieldRecursive(VisualElement root) {
        if (root == null) return null;

        if (root is TextField tf)
            return tf;

        foreach (var child in root.Children()) {
            var result = FindTextFieldRecursive(child);
            if (result != null)
                return result;
        }

        return null;
    }
}
*/

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

        if (!isDeserialized)
            AddAnswerUI();

        RegisterInputs();
    }


    #region Initialization

    /// <summary>Loads the correct answer template based on question type.</summary>
    private void LoadAnswerTemplate() {
        // var mapping = UnityEngine.Object.FindFirstObjectByType<QuestionUIMapping>();
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

    /// <summary>Recalculates indices after changes.</summary>
    private void RecalculateAnswerIndices() {
        for (int i = 0; i < _addedAnswers.Count; i++)
            _addedAnswers[i].UpdateIndex(i);

        _otherAnswerUI?.UpdateIndex(_addedAnswers.Count);
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