using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SurveyAnswerUIEditorImage : SurveyAnswerUIEditor {

    public event Action<int, string> OnAnswerImageChanged;
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

        var clickable = _answerElement.Q<VisualElement>("option-container");

        clickable?.RegisterCallback<ClickEvent>(evt => {
            ImageManager.Instance.AskForImageDialog((textureAsset) => {
                if (textureAsset != null) {
                    SetImage(textureAsset.ID);
                    OnAnswerImageChanged?.Invoke(_answerIndex, textureAsset.ID);
                }
            });
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
