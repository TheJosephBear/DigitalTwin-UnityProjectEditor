using SurveySystem;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIViewerImage : SurveyAnswerUIViewer {

    public event Action<int, int, bool> OnSelected;
    private VisualElement _imageDisplay;
    private VisualElement _selectionOverlay; // Optional: a border or checkmark to show it's selected
    private VisualElement _captionContainer;
    private Label _captionLabel;

    public SurveyAnswerUIViewerImage(VisualElement answerElement, int answerIndex, SurveyQuestionUIViewer questionUI, bool isOther)
        : base(answerElement, answerIndex, questionUI, isOther) {
        // RegisterAnswerEvents() is already called by the base constructor
        _selectionOverlay = _answerElement.Q<VisualElement>("selection-overlay"); // Ensure this exists in UXML if used
        _captionContainer = _answerElement.Q<VisualElement>("caption-container");
        _captionLabel = _answerElement.Q<Label>("option-caption");

        var letterLabel = _answerElement.Q<Label>("option-letter");
        if (letterLabel != null) letterLabel.text = AnswerImage.GetLetter(answerIndex);

        SetCaption(null);
    }

    protected override void RegisterAnswerEvents() {
        _imageDisplay = _answerElement.Q<VisualElement>("image");

        // In the viewer, clicking the whole container selects the answer
        var clickable = _answerElement.Q<VisualElement>("option-container");

        clickable?.RegisterCallback<ClickEvent>(evt => {
            if (evt.target is Button btn && btn.name == "enhance-image") {
                return;
            }
            OnSelected?.Invoke(_questionUIRef.QuestionID, AnswerIndex, true);
        });

        var enhanceBtn = _answerElement.Q<Button>("enhance-image");
        enhanceBtn?.RegisterCallback<ClickEvent>(evt => {
            evt.StopPropagation();
            if (_imageDisplay != null) {
                _questionUIRef?.EnhanceImage(_imageDisplay);
            }
        });
    }

    public void SetImage(string imageId) {
        TextureAsset asset = string.IsNullOrEmpty(imageId) ? null : ImageManager.Instance.GetTextureAssetByID(imageId);
        bool hasImage = asset != null && asset.Texture != null;
        if (hasImage) {
            _imageDisplay.style.backgroundImage = new StyleBackground(asset.Texture);
        }

        // Without an image show only a small grey placeholder, nothing to enlarge
        var placeholder = _answerElement.Q<VisualElement>("image-placeholder");
        if (placeholder != null) placeholder.style.display = hasImage ? DisplayStyle.None : DisplayStyle.Flex;
        var enhanceBtn = _answerElement.Q<Button>("enhance-image");
        if (enhanceBtn != null) enhanceBtn.style.display = hasImage ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetCaption(string caption) {
        bool hasCaption = !string.IsNullOrWhiteSpace(caption);
        if (_captionLabel != null) _captionLabel.text = hasCaption ? caption.Trim() : "";
        if (_captionContainer != null) _captionContainer.style.display = hasCaption ? DisplayStyle.Flex : DisplayStyle.None;    }

    public void SetSelected(bool selected) {
        if (selected) {
            _answerElement.Children().First().AddToClassList("image-option-card--selected"); // Use USS to show selection
        } else {
            _answerElement.Children().First().RemoveFromClassList("image-option-card--selected");
        }
    }
}
