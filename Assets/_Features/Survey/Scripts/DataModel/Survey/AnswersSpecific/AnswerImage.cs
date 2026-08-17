using System;
using UnityEngine;

namespace SurveySystem {
    [Serializable]
    public class AnswerImage : AnswerBase {
        public string ImageID;
        public string ImageId;

        public string GetImageId() {
            return !string.IsNullOrEmpty(ImageID) ? ImageID : (!string.IsNullOrEmpty(ImageId) ? ImageId : "");
        }
    }
}