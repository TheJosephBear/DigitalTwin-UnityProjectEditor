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
        Debug.Log("Set image callled " + ImageID);
        _root.Q<VisualElement>("camera-view").style.display = DisplayStyle.None;
        if (ImageID == "" || ImageID == null) return;

        TextureAsset textureAsset = ImageManager.Instance.GetTextureAssetByID(ImageID);
        if (textureAsset == null) return;

        _root.Q<VisualElement>("camera-view").style.display = DisplayStyle.Flex;
        SetRenderedImage(textureAsset.Texture);
    }
    public override void SetTitle(string title) {
        _root.Q<Label>("question-title").text = title;
    }

    public override void SetDescription(string desc) {
        _root.Q<Label>("question-description").text = desc;
    }

}