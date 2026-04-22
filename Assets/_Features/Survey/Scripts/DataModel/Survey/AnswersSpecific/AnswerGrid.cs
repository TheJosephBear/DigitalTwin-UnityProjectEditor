using UnityEngine;

namespace SurveySystem {
    [System.Serializable]
    public class AnswerGrid : AnswerBase {
        public int Row;
        public int Collumn;
        public bool IsSelected;

        public AnswerGrid(int row, int collumn) {
            Row = row;
            Collumn = collumn;
        }
    }
}