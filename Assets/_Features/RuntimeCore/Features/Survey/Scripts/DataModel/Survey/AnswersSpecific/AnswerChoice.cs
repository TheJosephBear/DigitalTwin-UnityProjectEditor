using UnityEngine;

namespace SurveySystem {
    [System.Serializable]
    public class AnswerChoice : AnswerBase {
        public string Text;
        public bool IsSelected;

    //    public override object GetValue() => IsSelected;
    }
}