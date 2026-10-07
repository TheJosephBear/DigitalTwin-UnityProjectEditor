using SurveySystem;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIEditorImage : SurveyAnswerUIEditor {

    public event Action<int, string> OnAnswerImageChanged;
    public event Action<int, string> OnCaptionChanged;
    private VisualElement _imageDisplay;
    private Label _letterLabel;
    private TextField _captionField;

    public SurveyAnswerUIEditorImage(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditor questionUI, bool isOther)
        : base(answerElement, answerIndex, questionUI, isOther) {
        // RegisterAnswerEvents() is already called by the base constructor
        RefreshLetter();
    }

    protected override void RegisterAnswerEvents() {
        _imageDisplay = _answerElement.Q<VisualElement>("image");
        _letterLabel = _answerElement.Q<Label>("option-letter");
        _captionField = _answerElement.Q<TextField>("option-caption");

        // Register buttons for Move Up/Down/Delete (inherited logic, handles delete-option-button)
        RegisterModalButtonEvents(_answerElement);

        _imageDisplay?.RegisterCallback<ClickEvent>(evt => {
            if (evt.target is Button btn && (btn.name == "enhance-image" || btn.name == "delete-option-button")) {
                return;
            }
            ImageManager.Instance.AskForImageDialog((textureAsset) => {
                if (textureAsset != null) {
                    SetImage(textureAsset.ID);
                    OnAnswerImageChanged?.Invoke(_answerIndex, textureAsset.ID);
                }
            });
        });

        var enhanceBtn = _answerElement.Q<Button>("enhance-image");
        enhanceBtn?.RegisterCallback<ClickEvent>(evt => {
            evt.StopPropagation();
            if (_imageDisplay != null) {
                _questionUIRef?.EnhanceImage(_imageDisplay);
            }
        });

        _captionField?.RegisterValueChangedCallback(evt => {
            OnCaptionChanged?.Invoke(_answerIndex, evt.newValue);
        });
    }

    public override void UpdateIndex(int newIndex) {
        base.UpdateIndex(newIndex);
        RefreshLetter();
    }

    void RefreshLetter() {
        if (_letterLabel != null) {
            _letterLabel.text = AnswerImage.GetLetter(_answerIndex);
        }
    }

    public void SetCaption(string caption) {
        _captionField?.SetValueWithoutNotify(caption ?? "");
    }

    public void SetImage(string imageId) {
        if (string.IsNullOrEmpty(imageId)) return;

        TextureAsset asset = ImageManager.Instance.GetTextureAssetByID(imageId);
        if (asset != null && asset.Texture != null) {
            _imageDisplay.style.backgroundImage = new StyleBackground(asset.Texture);
        }
    }
}
