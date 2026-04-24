using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIGrid {
    // Row/Column label

    private VisualElement _answerElement;
    private int _answerIndex;
    private SurveyQuestionUIEditor _questionUIRef;

    // Track currently open modal
    private VisualElement _currentlyOpenModal = null;
    private VisualElement _originalParent = null;
    private int _originalIndex = -1;
    private bool _isOther = false;

    public VisualElement AnswerElement => _answerElement;
    public int AnswerIndex => _answerIndex;

    #region Events

    public event Action<int, int, string> OnTextChanged;
    public event Action<int, string> OnAnswerSelected;

    #endregion

    public SurveyAnswerUIGrid(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditorGrid questionUI, bool isOther) {
    }


    public void UpdateIndex(int newIndex) {
        _answerIndex = newIndex;
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

    private void OnRootPointerDown(PointerDownEvent evt) {
        // Only proceed if a modal is open
        if (_currentlyOpenModal != null && _currentlyOpenModal.style.display == DisplayStyle.Flex) {
            // Check if the click target is outside the modal container
            if (!_currentlyOpenModal.ContainsPoint(_currentlyOpenModal.WorldToLocal(evt.position))) {
                HideCurrentModal();
            }
        }
    }
}
