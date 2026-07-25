using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIEditorImage : SurveyAnswerUIEditor {

    public event Action<int, string> OnAnswerImageChanged;
    public event Action<int> OnRemoveClicked;
    private VisualElement _imageDisplay;

    public SurveyAnswerUIEditorImage(VisualElement answerElement, int answerIndex, SurveyQuestionUIEditor questionUI, bool isOther)
        : base(answerElement, answerIndex, questionUI, isOther) {

        _imageDisplay = _answerElement.Q<VisualElement>("image");
        Debug.Log("Image display is: "+ _imageDisplay.name);    
        RegisterAnswerEvents();
    }

    protected override void RegisterAnswerEvents() {
        // Register buttons for Move Up/Down/Delete (inherited logic)
        RegisterModalButtonEvents(_answerElement);

        var clickable = _answerElement.Q<VisualElement>("image");

        clickable?.RegisterCallback<ClickEvent>(evt => {
            if (evt.target is Button btn && btn.name == "enhance-image") {
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

        var deleteButton = _answerElement.Q<Button>("delete-option-button");

        deleteButton?.RegisterCallback<ClickEvent>(evt => {
            OnRemoveClicked?.Invoke(AnswerIndex);
        });
    }

    public void SetImage(string imageId) {
        if (string.IsNullOrEmpty(imageId)) return;

        TextureAsset asset = ImageManager.Instance.GetTextureAssetByID(imageId);
        if (asset != null && asset.Texture != null) {
            _imageDisplay.style.backgroundImage = new StyleBackground(asset.Texture);
        }
    }
}
