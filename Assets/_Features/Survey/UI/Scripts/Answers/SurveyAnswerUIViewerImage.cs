using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIViewerImage : SurveyAnswerUIViewer {

    public event Action<int, int, bool> OnSelected;
    private VisualElement _imageDisplay;
    private VisualElement _selectionOverlay; // Optional: a border or checkmark to show it's selected

    public SurveyAnswerUIViewerImage(VisualElement answerElement, int answerIndex, SurveyQuestionUIViewer questionUI, bool isOther)
        : base(answerElement, answerIndex, questionUI, isOther) {

        _imageDisplay = _answerElement.Q<VisualElement>("image");
        _selectionOverlay = _answerElement.Q<VisualElement>("selection-overlay"); // Ensure this exists in UXML if used

        RegisterAnswerEvents();
    }

    protected override void RegisterAnswerEvents() {
        // In the viewer, clicking the whole container selects the answer
        var clickable = _answerElement.Q<VisualElement>("option-container");

        clickable?.RegisterCallback<ClickEvent>(evt => {
            OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
        });
    }

    public void SetImage(string imageId) {
        if (string.IsNullOrEmpty(imageId)) return;

        TextureAsset asset = ImageManager.Instance.GetTextureAssetByID(imageId);
        if (asset != null && asset.Texture != null) {
            _imageDisplay.style.backgroundImage = new StyleBackground(asset.Texture);
        }
    }

    public void SetSelected(bool selected) {
        if (selected) {
            _answerElement.AddToClassList("answer-selected"); // Use USS to show selection
        } else {
            _answerElement.RemoveFromClassList("answer-selected");
        }
    }
}