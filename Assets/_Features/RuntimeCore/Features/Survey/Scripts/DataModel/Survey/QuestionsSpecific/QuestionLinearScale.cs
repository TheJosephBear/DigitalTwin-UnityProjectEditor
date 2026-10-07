using System.Collections.Generic;

namespace SurveySystem {
    public class QuestionLinearScale : QuestionBase {
        public int Min = 1;
        public int Max = 5;
        public string ScaleType = "1 - 5";

        public QuestionLinearScale(int ID)
            : base(ID, QuestionType.LinearScale) {
            MultipleAnswersAllowed = false;
        }

        public void SetScaleRange(string scaleType, int min, int max) {
            ScaleType = scaleType;
            Min = min;
            Max = max;
        }

        public override AnswerBase AddNewAnswer() {
            AnswerBase answer = new AnswerString {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = false
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public override AnswerBase AddNewAnswer(bool isOther) {
            AnswerBase answer = new AnswerString {
                Idx = _answers.Count,
                Text = string.Empty,
                IsOther = isOther
            };

            _answers.Add(answer);
            ActiveAnswer = answer;
            return answer;
        }

        public override SerializableQuestion Serialize() {
            SerializableQuestion serializable = base.Serialize();
            serializable.Min = Min;
            serializable.Max = Max;
            serializable.ScaleType = ScaleType;
            return serializable;
        }

        public override QuestionBase Deserialize(SerializableQuestion serializable) {
            base.Deserialize(serializable);
            if (!string.IsNullOrEmpty(serializable.ScaleType)) {
                ScaleType = serializable.ScaleType;
                var (parsedMin, parsedMax) = SurveyQuestionUIEditorLinearScale.ParseScaleRange(ScaleType);
                Min = serializable.Min != 0 || serializable.Max != 0 ? serializable.Min : parsedMin;
                Max = serializable.Max != 0 ? serializable.Max : parsedMax;
            } else {
                Min = serializable.Min != 0 || serializable.Max != 0 ? serializable.Min : 1;
                Max = serializable.Max != 0 ? serializable.Max : 5;
                ScaleType = $"{Min} - {Max}";
            }
            return this;
        }
    }
}