using UnityEngine;
using UnityEngine.UIElements;
using SurveySystem;
using System.Collections.Generic;
using UnityEditor;
using System;

public class SurveyQuestionUI {
    public int _questionID;
    private SurveyBuildingUI _surveyBuildingUIReff;
    private VisualElement _root;
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

    public SurveyQuestionUI(VisualElement root, int questionId, SurveyBuildingUI surveyBuildingUI, QuestionType questionType, List<SerializableViewPoint> viewPoints) {
        _root = root;
        _questionID = questionId;
        _surveyBuildingUIReff = surveyBuildingUI;
        _questionType = questionType;
        _viewPoints = viewPoints;

        // Get the answer template from QuestionUIMapping
        QuestionUIMapping mapping = UnityEngine.Object.FindFirstObjectByType<QuestionUIMapping>();
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
        AddAnswerUI(); // Add the first answer UI element by default

        RegisterInputs();
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

            answerElement.Q<CustomRadioButton>().Placeholder = "Jin�"; // Set label to "Other"
            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, _surveyBuildingUIReff, this);
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

            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, _surveyBuildingUIReff, this);
            _addedAnswers.Add(answerUI);
        }

    }

    private void RegisterInputs() {
        var questionTitleField = _root.Q<TextField>("question-title");
        var questionDescriptionField = _root.Q<TextField>("question-description");
        var addOptionButton = _root.Q<Button>("add-option-button");
        var addOptionOtherButton = _root.Q<Button>("add-other-option-button");
        var cameraViewDropdown = _root.Q<DropdownField>("camera-view-dropdown");
        var editQuestionButton = _root.Q<VisualElement>("edit-question-button");

        questionTitleField.RegisterValueChangedCallback(evt => {
            _surveyBuildingUIReff.HandleQuestionTitleChanged(_questionID, evt.newValue);
        });

        questionDescriptionField.RegisterValueChangedCallback(evt => {
            _surveyBuildingUIReff.HandleQuestionDescriptionChanged(_questionID, evt.newValue);
        });

        addOptionButton.clicked += () => {
            _surveyBuildingUIReff.HandleAnswerAdded(_questionID);
            AddAnswerUI(false);
        };

        if (addOptionOtherButton != null) {
            addOptionOtherButton.clicked += () => {
                // Only allow one "Other" answer per question
                if (_otherAnswerUI == null) {
                    _surveyBuildingUIReff.HandleAnswerOtherAdded(_questionID);
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
                    _surveyBuildingUIReff.HandleQuestionViewPointSelected(_questionID, _viewPoints[index].ID);
                }
            });
        }

        PopulateCameraViewDropdown(cameraViewDropdown);
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
            _surveyBuildingUIReff.HandleQuestionViewPointSelected(_questionID, _viewPoints[0].ID);
        }
    }

    private void AddAnswerUI(bool isOther = false) {
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

            answerElement.Q<CustomRadioButton>().Placeholder = "Jin�"; // Set label to "Other"
            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, _surveyBuildingUIReff, this);
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

            // Create SurveyAnswerUI instance to manage this answer
            SurveyAnswerUI answerUI = new SurveyAnswerUI(answerElement, answerIndex, _surveyBuildingUIReff, this);
            _addedAnswers.Add(answerUI);
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

        if (deleteButton != null) {
            deleteButton.clicked += () => {
                int index = _surveyBuildingUIReff.GetQuestionIndex(this);
                if (index >= 0) {
                    HideQuestionModal();
                    _surveyBuildingUIReff.HandleQuestionDeleted(index);
                }
            };
        }

        if (moveUpButton != null) {
            moveUpButton.clicked += () => {
                int index = _surveyBuildingUIReff.GetQuestionIndex(this);
                if (index >= 0) {
                    _surveyBuildingUIReff.HandleQuestionMoved(index, -1);
                }
                HideQuestionModal();
            };
        }

        if (moveDownButton != null) {
            moveDownButton.clicked += () => {
                int index = _surveyBuildingUIReff.GetQuestionIndex(this);
                if (index >= 0) {
                    _surveyBuildingUIReff.HandleQuestionMoved(index, 1);
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
            _surveyBuildingUIReff.HandleAnswerRemoved(answer);

            // Remove "Other" answer from UI
            _optionsList.Remove(_otherAnswerUI.AnswerElement);
            _otherAnswerUI = null;
            return;
        }

        if (answerIndex >= _addedAnswers.Count) return;

        var answerUI = _addedAnswers[answerIndex];

        // Create answer object for removal from data model
        var answerToRemove = new AnswerBase { Idx = answerIndex };
        _surveyBuildingUIReff.HandleAnswerRemoved(answerToRemove);

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
