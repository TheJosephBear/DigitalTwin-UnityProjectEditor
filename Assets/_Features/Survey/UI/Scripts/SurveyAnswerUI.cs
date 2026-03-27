using SurveySystem;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUI {
    private VisualElement _answerElement;
    private int _answerIndex;
    private SurveyQuestionUI _questionUIRef;

    // Track currently open modal
    private VisualElement _currentlyOpenModal = null;
    private VisualElement _originalParent = null;
    private int _originalIndex = -1;

    public VisualElement AnswerElement => _answerElement;
    public int AnswerIndex => _answerIndex;

    #region Events

    public event Action<int, int, string> OnTextChanged;
    public event Action<int, string> OnAnswerSelected;

    #endregion

    public SurveyAnswerUI(VisualElement answerElement, int answerIndex, SurveyQuestionUI questionUI) {
        _answerElement = answerElement;
        _answerIndex = answerIndex;
        _questionUIRef = questionUI;

        RegisterAnswerEvents();
    }

    public void UpdateIndex(int newIndex) {
        _answerIndex = newIndex;
    }

    private void RegisterAnswerEvents() {
        // For each option/answer
        var editAnswerButton = _answerElement.Q<VisualElement>("edit-option-button");

        if (editAnswerButton != null) {
            editAnswerButton.RegisterCallback<ClickEvent>(evt => {
                // Toggle visibility of edit options container
                var editContainer = _answerElement.Q<VisualElement>("edit-option-container");
                if (editContainer != null) {
                    bool isCurrentlyHidden = editContainer.style.display != DisplayStyle.Flex;

                    // Close any previously open modal in the question
                    _questionUIRef.CloseCurrentModal();

                    // Don't set display here - it will be set after positioning
                    if (!isCurrentlyHidden) {
                        editContainer.style.display = DisplayStyle.None;
                    }

                    // Register modal button events and outside click handler only when opening the modal
                    if (isCurrentlyHidden) {
                        editContainer.parent.style.overflow = Overflow.Visible;

                        // Find question-container up the hierarchy (up to 10 parents)
                        VisualElement current = editContainer.parent;
                        int maxParentsToCheck = 10;
                        VisualElement questionContainer = null;

                        while (current != null && maxParentsToCheck > 0) {
                            if (current.name == "question-container") {
                                questionContainer = current;
                                current.style.overflow = Overflow.Visible;
                                break;
                            }
                            current = current.parent;
                            maxParentsToCheck--;
                        }

                        // Move editContainer to questionContainer to prevent clipping
                        if (questionContainer != null) {
                            // First make it visible to get accurate bounds
                            editContainer.style.display = DisplayStyle.Flex;

                            // Store original parent info for restoration
                            _originalParent = editContainer.parent;
                            _originalIndex = _originalParent.IndexOf(editContainer);

                            // Store the button's world position to align relative to it
                            Rect buttonBound = editAnswerButton.worldBound;
                            Rect questionContainerBound = questionContainer.worldBound;

                            // Remove from current parent and add to question container
                            editContainer.RemoveFromHierarchy();
                            questionContainer.Add(editContainer);

                            // Calculate position to align the modal's right edge with button's right edge
                            // and position it below the button
                            float buttonRightEdge = buttonBound.x + buttonBound.width;
                            float rightDistance = questionContainerBound.width - (buttonRightEdge - questionContainerBound.x);
                            float topPosition = buttonBound.y + buttonBound.height - questionContainerBound.y;

                            // Set position aligned to right
                            editContainer.style.position = Position.Absolute;
                            editContainer.style.right = new StyleLength(new Length(rightDistance, LengthUnit.Pixel));
                            editContainer.style.top = new StyleLength(new Length(topPosition, LengthUnit.Pixel));
                            editContainer.style.left = StyleKeyword.Auto;
                            editContainer.style.bottom = StyleKeyword.Auto;

                            // Bring to front to ensure it renders on top
                            editContainer.BringToFront();
                        } else {
                            // Fallback if question-container not found
                            editContainer.style.display = DisplayStyle.Flex;
                        }

                        _currentlyOpenModal = editContainer;
                        RegisterModalButtonEvents(editContainer);
                        RegisterOutsideClickHandler(editContainer);
                    } else {
                        _currentlyOpenModal = null;
                        UnregisterOutsideClickHandler();
                    }
                }

                // Stop event propagation to prevent immediate closing
                evt.StopPropagation();
            });
        }

        // Register text field change event if exists
        var answerTextField = _answerElement.Q<TextField>();
        if (answerTextField != null) {
            answerTextField.RegisterValueChangedCallback(evt => {
                // (!) this didnt work -> works with the created answerbase instance instead of the expected one in the survey

                // Create a minimal AnswerBase object for the callback
                //    var answer = new AnswerBase { Idx = _answerIndex, Text = evt.newValue };
                //    _surveyUIHandler.HandleAnswerTextChanged(answer, evt.newValue);

                // (!) this works for any added answer via (add answer)button but not the one that is there once the question is added
                //    _surveyUIHandler.HandleAnswerTextChanged(_questionUIRef.QuestionID, _answerIndex, evt.newValue);
                OnTextChanged.Invoke(_questionUIRef.QuestionID, _answerIndex, evt.newValue);
            });
        }
    }

    private void OnRootPointerDown(PointerDownEvent evt) {
        // Only proceed if a modal is open
        if (_currentlyOpenModal != null && _currentlyOpenModal.style.display == DisplayStyle.Flex) {
            // Check if the click target is outside the modal container
            if (!_currentlyOpenModal.ContainsPoint(_currentlyOpenModal.WorldToLocal(evt.position))) {
                HideCurrentModal();
            }
        }
    }

    private void RegisterOutsideClickHandler(VisualElement modal) {
        // Get the root element to register the global click handler
        VisualElement root = modal;
        while (root.parent != null) {
            root = root.parent;
        }

        // Register callback on the global root to detect clicks anywhere in the document
        root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    }

    private void UnregisterOutsideClickHandler() {
        if (_currentlyOpenModal != null) {
            // Get the root element to unregister the global click handler
            VisualElement root = _currentlyOpenModal;
            while (root.parent != null) {
                root = root.parent;
            }

            root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
        }
    }

    public void HideCurrentModal() {
        if (_currentlyOpenModal != null) {
            _currentlyOpenModal.style.display = DisplayStyle.None;

            // Restore element to original parent if it was moved
            if (_originalParent != null) {
                _currentlyOpenModal.RemoveFromHierarchy();
                _originalParent.Insert(_originalIndex, _currentlyOpenModal);
                _originalParent = null;
                _originalIndex = -1;
            }

            UnregisterOutsideClickHandler();
            _currentlyOpenModal = null;
        }
    }

    private void RegisterModalButtonEvents(VisualElement editContainer) {
        var deleteAnswerButton = editContainer.Q<Button>("delete-option-button");
        var moveUpButton = editContainer.Q<Button>("move-up-button");
        var moveDownButton = editContainer.Q<Button>("move-down-button");

        if (deleteAnswerButton != null) {
            deleteAnswerButton.clicked += () => {
                _questionUIRef.DeleteAnswer(_answerIndex);
                HideCurrentModal();
            };
        }

        if (moveUpButton != null) {
            moveUpButton.clicked += () => {
                _questionUIRef.MoveAnswerUp(_answerIndex);
                HideCurrentModal();
            };
        }

        if (moveDownButton != null) {
            moveDownButton.clicked += () => {
                _questionUIRef.MoveAnswerDown(_answerIndex);
                HideCurrentModal();
            };
        }
    }
}
