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

        /// Letter of the option by its position: 0 -> A, 1 -> B, ..., 25 -> Z, 26 -> AA, ...
        public static string GetLetter(int idx) {
            if (idx < 0) return "";
            string letter = "";
            idx++;
            while (idx > 0) {
                idx--;
                letter = (char)('A' + idx % 26) + letter;
                idx /= 26;
            }
            return letter;
        }

        /// Label saved to response data and Excel: "B" or "B – caption" (Text holds the optional caption).
        public string GetLabel() {
            string letter = GetLetter(Idx);
            return string.IsNullOrWhiteSpace(Text) ? letter : $"{letter} – {Text.Trim()}";
        }
    }
}
