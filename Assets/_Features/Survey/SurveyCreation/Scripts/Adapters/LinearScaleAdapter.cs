using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
using UnityEngine;


namespace QuestionnaireToolkit {
    public class LinearScaleAdapter : IQuestionAdapter {

        QTLinearScale _linearScale;

        public LinearScaleAdapter(QTLinearScale linearScale) {
            _linearScale = linearScale;
        }

        public void SetQuestionText(string question) {
            _linearScale.question = question;
            _linearScale.OnValidate();
        }

        public void SetOptionText(int optionIndex, string optionText) {
            _linearScale.selectedIndex = optionIndex;
            _linearScale.answerOption = optionText;

            var oldName = _linearScale.options[optionIndex].name;
            var oldValue = oldName.Split('_')[0];
            _linearScale.answerValue = oldValue;
            _linearScale.EditOption();
        }

        public void AddOption() {
            _linearScale.AddOption(scriptBased: true, a_value: "1", a_option: "");
        }

        public void RemoveOption(int optionIndex) {
            _linearScale.DeleteItem(GetOptionsCount() - 1, optionIndex);
            _linearScale.OnValidate();
        }

        public List<QTOptionsData> GetOptionsData() {
            List<QTOptionsData> data = new List<QTOptionsData>();
            int idx = 0;

            foreach (var option in _linearScale.options) {
                string[] parts = option.name.Split('_');
                string value = parts.Length > 0 ? parts[0] : "";
                string text = parts.Length > 1 ? parts[1] : "";

                data.Add(new QTOptionsData {
                    idx = idx,
                    questionText = text
                });
                idx++;
            }
            return data;
        }

        public int GetOptionsCount() {
            return _linearScale.options.Count;
        }

        public string GetQuestionText() {
            return _linearScale.question;
        }
    }
}