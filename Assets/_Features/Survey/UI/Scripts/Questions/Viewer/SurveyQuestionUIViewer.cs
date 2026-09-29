using SurveySystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class SurveyQuestionUIViewer : SurveyQuestionUIBase {

    public SurveyQuestionUIViewer(VisualElement root, int questionId, QuestionType questionType, List<SerializableViewPoint> viewPoints, SurveyUIBuilder uiBuilder) 
        : base(root, questionId, questionType, viewPoints, uiBuilder) {
    
    }

    public override void SetImageRender() {
        Debug.Log("Set image called " + ImageID);
        var questionImage = _root.Q<VisualElement>("question-image");
        if (questionImage != null) {
            questionImage.style.display = DisplayStyle.None;
            questionImage.style.backgroundImage = null;
        }

        if (string.IsNullOrEmpty(ImageID)) return;

        TextureAsset textureAsset = ImageManager.Instance.GetTextureAssetByID(ImageID);
        if (textureAsset == null) return;

        if (questionImage != null) {
            questionImage.style.display = DisplayStyle.Flex;
            questionImage.style.backgroundImage = Background.FromTexture2D((Texture2D)textureAsset.Texture);
        }
    }

    protected override void RegisterButtons() {
        var cameraView = _root.Q<VisualElement>("camera-view");
        if (cameraView != null) {
            var enhanceCamBtn = cameraView.Q<Button>("enhance-image");
            if (enhanceCamBtn != null) {
                enhanceCamBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    var currentCamView = _root.Q<VisualElement>("camera-view");
                    EnhanceImage(currentCamView);
                });
            }
        }

        var questionImage = _root.Q<VisualElement>("question-image");
        if (questionImage != null) {
            var enhanceImgBtn = questionImage.Q<Button>("enhance-image");
            if (enhanceImgBtn != null) {
                enhanceImgBtn.RegisterCallback<ClickEvent>(evt => {
                    evt.StopPropagation();
                    var currentQuestionImg = _root.Q<VisualElement>("question-image");
                    EnhanceImage(currentQuestionImg);
                });
            }
        }
    }

    public override void SetTitle(string title) {
        var label = _root.Q<Label>("question-title");
        if (label != null) {
            label.text = title;
        }
    }

    public void SetRequired(bool required) {
        var requiredSymbol = _root.Q<VisualElement>("required-symbol");
        if (requiredSymbol != null) {
            requiredSymbol.style.display = required ? DisplayStyle.Flex : DisplayStyle.None;
        }
        _root.Q<VisualElement>("question-container")?.EnableInClassList("question-card--required", required);
    }

    // Highlights the question (and its unanswered parts) when a required question isn't filled
    public virtual void SetValidationError(bool invalid, QuestionValidationResult result) {
        _root.Q<VisualElement>("question-container")?.EnableInClassList("question-card--invalid", invalid);
    }

    public override void SetDescription(string desc) {
        var label = _root.Q<Label>("question-description");
        if (label != null) {
            string trimmed = desc?.Trim() ?? string.Empty;
            label.text = trimmed;
            label.style.display = string.IsNullOrEmpty(trimmed) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }

}