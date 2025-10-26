using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
using TMPro;
using UnityEngine;


namespace QuestionnaireToolkit {
    public class MultipleChoiceAdapter : IQuestionAdapter {

        QTMultipleChoice _multipleChoice;

        public MultipleChoiceAdapter(QTMultipleChoice multipleChoice) {
            _multipleChoice = multipleChoice;
        }

        public void SetQuestionText(string question) {
            _multipleChoice.question = question;
            _multipleChoice.OnValidate();
        }

        public void AddOption() {
            _multipleChoice.answerOption = "";
            _multipleChoice.answerValue = _multipleChoice.options.Count.ToString();
            _multipleChoice.AddOption();
        }

        public void SetOptionText(int optionIndex, string optionText) {
            _multipleChoice.selectedIndex = optionIndex;
            _multipleChoice.answerOption = optionText;

            var oldName = _multipleChoice.options[optionIndex].name;
            var oldValue = oldName.Split('_')[0];
            _multipleChoice.answerValue = oldValue;
            _multipleChoice.EditOption();
        }

        public void RemoveOption(int optionIndex) {
            _multipleChoice.DeleteItem(GetOptionsCount()-1, optionIndex);
            _multipleChoice.OnValidate();
        }

        public List<QTOptionsData> GetOptionsData() {
            List<QTOptionsData> data = new List<QTOptionsData>();
            int idx = 0;

            foreach(var option in _multipleChoice.options) {
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
            return _multipleChoice.options.Count;
        }

        public string GetQuestionText() {
            return _multipleChoice.question;
        }
    }
}