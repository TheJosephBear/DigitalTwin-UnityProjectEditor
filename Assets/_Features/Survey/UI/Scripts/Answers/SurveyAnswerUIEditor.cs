using UnityEngine;
using UnityEngine.UIElements;

public abstract class SurveyAnswerUIEditor : SurveyAnswerUIBase {

    // Track currently open modal
    protected VisualElement _currentlyOpenModal = null;
    protected VisualElement _originalParent = null;
    protected int _originalIndex = -1;

    public SurveyAnswerUIEditor(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditor questionUI, bool isOther) 
        : base(answerElement, answerIndex, questionUI, isOther) {

    }

    #region Modal Logic

    //protected void RegisterEditButtonWithModal() {
    //    var editAnswerButton = _answerElement.Q<VisualElement>("edit-option-button");

    //    if (editAnswerButton != null) {
    //        editAnswerButton.RegisterCallback<ClickEvent>(evt => {
    //            var editContainer = _answerElement.Q<VisualElement>("edit-option-container");

    //            if (editContainer != null) {
    //                bool isCurrentlyHidden = editContainer.style.display != DisplayStyle.Flex;

    //                if(_questionUIRef is SurveyQuestionUIEditor questionEditor)
    //                    questionEditor.CloseCurrentModal();

    //                if (!isCurrentlyHidden) {
    //                    editContainer.style.display = DisplayStyle.None;
    //                }

    //                if (isCurrentlyHidden) {
    //                    editContainer.parent.style.overflow = Overflow.Visible;

    //                    VisualElement current = editContainer.parent;
    //                    int maxParentsToCheck = 10;
    //                    VisualElement questionContainer = null;

    //                    while (current != null && maxParentsToCheck > 0) {
    //                        if (current.name == "question-container") {
    //                            questionContainer = current;
    //                            current.style.overflow = Overflow.Visible;
    //                            break;
    //                        }
    //                        current = current.parent;
    //                        maxParentsToCheck--;
    //                    }

    //                    if (questionContainer != null) {
    //                        editContainer.style.display = DisplayStyle.Flex;

    //                        _originalParent = editContainer.parent;
    //                        _originalIndex = _originalParent.IndexOf(editContainer);

    //                        Rect buttonBound = editAnswerButton.worldBound;
    //                        Rect questionContainerBound = questionContainer.worldBound;

    //                        editContainer.RemoveFromHierarchy();
    //                        questionContainer.Add(editContainer);

    //                        float buttonRightEdge = buttonBound.x + buttonBound.width;
    //                        float rightDistance = questionContainerBound.width - (buttonRightEdge - questionContainerBound.x);
    //                        float topPosition = buttonBound.y + buttonBound.height - questionContainerBound.y;

    //                        editContainer.style.position = Position.Absolute;
    //                        editContainer.style.right = new StyleLength(new Length(rightDistance, LengthUnit.Pixel));
    //                        editContainer.style.top = new StyleLength(new Length(topPosition, LengthUnit.Pixel));
    //                        editContainer.style.left = StyleKeyword.Auto;
    //                        editContainer.style.bottom = StyleKeyword.Auto;

    //                        editContainer.BringToFront();
    //                    } else {
    //                        editContainer.style.display = DisplayStyle.Flex;
    //                    }

    //                    _currentlyOpenModal = editContainer;
    //                    RegisterModalButtonEvents(editContainer);
    //                    RegisterOutsideClickHandler(editContainer);
    //                } else {
    //                    _currentlyOpenModal = null;
    //                    UnregisterOutsideClickHandler();
    //                }
    //            }

    //            evt.StopPropagation();
    //        });
    //    }
    //}

    //protected void OnRootPointerDown(PointerDownEvent evt) {
    //    // Only proceed if a modal is open
    //    if (_currentlyOpenModal != null && _currentlyOpenModal.style.display == DisplayStyle.Flex) {
    //        // Check if the click target is outside the modal container
    //        if (!_currentlyOpenModal.ContainsPoint(_currentlyOpenModal.WorldToLocal(evt.position))) {
    //            HideCurrentModal();
    //        }
    //    }
    //}

    //protected void RegisterOutsideClickHandler(VisualElement modal) {
    //    // Get the root element to register the global click handler
    //    VisualElement root = modal;
    //    while (root.parent != null) {
    //        root = root.parent;
    //    }

    //    // Register callback on the global root to detect clicks anywhere in the document
    //    root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    //}

    //protected void UnregisterOutsideClickHandler() {
    //    if (_currentlyOpenModal != null) {
    //        // Get the root element to unregister the global click handler
    //        VisualElement root = _currentlyOpenModal;
    //        while (root.parent != null) {
    //            root = root.parent;
    //        }

    //        root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
    //    }
    //}

    //public override void HideCurrentModal() {
    //    if (_currentlyOpenModal != null) {
    //        _currentlyOpenModal.style.display = DisplayStyle.None;

    //        // Restore element to original parent if it was moved
    //        if (_originalParent != null) {
    //            _currentlyOpenModal.RemoveFromHierarchy();
    //            _originalParent.Insert(_originalIndex, _currentlyOpenModal);
    //            _originalParent = null;
    //            _originalIndex = -1;
    //        }

    //        UnregisterOutsideClickHandler();
    //        _currentlyOpenModal = null;
    //    }
    //}

    public virtual void SetMoveButtonsEnabled(bool canMoveUp, bool canMoveDown) {
        var moveUpButton = _answerElement.Q<Button>("move-up-button");
        var moveDownButton = _answerElement.Q<Button>("move-down-button");

        moveUpButton?.SetEnabled(canMoveUp);
        moveDownButton?.SetEnabled(canMoveDown);
    }

    protected void RegisterModalButtonEvents(VisualElement answerRoot) {
        var deleteAnswerButton = answerRoot.Q<Button>("delete-option-button");
        var moveUpButton = answerRoot.Q<Button>("move-up-button");
        var moveDownButton = answerRoot.Q<Button>("move-down-button");

        if (deleteAnswerButton != null) {
            deleteAnswerButton.clicked += () => {
                if (_questionUIRef is SurveyQuestionUIEditorString questionEditorString)
                    questionEditorString.DeleteAnswer(_answerIndex);
                else if (_questionUIRef is SurveyQuestionUIEditorLinearScale questionEditorLinearScale)
                    questionEditorLinearScale.DeleteAnswer(_answerIndex);
                else if (_questionUIRef is SurveyQuestionUIEditorImage questionEditorImage)
                    questionEditorImage.DeleteAnswer(_answerIndex);
            };
        }

        if (moveUpButton != null) {
            moveUpButton.clicked += () => {
                if (_questionUIRef is SurveyQuestionUIEditor questionEditor)
                    questionEditor.MoveAnswerUp(_answerIndex);
            };
        }

        if (moveDownButton != null) {
            moveDownButton.clicked += () => {
                if (_questionUIRef is SurveyQuestionUIEditor questionEditor)
                    questionEditor.MoveAnswerDown(_answerIndex);
            };
        }
    }

    #endregion

}
