using UnityEngine;

namespace SurveySystem {
    [System.Serializable]
    public class AnswerString : AnswerBase {
        public string TextRight; // Linear scale: label at the right end of the row (Text is the left one)
    }
}