using UnityEngine;

namespace SurveySystem {
    [System.Serializable]
    public class AnswerGrid : AnswerBase {
        public int Row;
        public int Column;
        public bool IsSelected;

     //   public override object GetValue() => new Vector2Int(Row, Column);
    }
}