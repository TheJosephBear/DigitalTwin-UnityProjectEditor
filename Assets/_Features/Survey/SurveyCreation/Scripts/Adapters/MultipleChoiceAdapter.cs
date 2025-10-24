using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
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

        public int GetOptionsCount() {
            return _multipleChoice.options.Count;
        }
    }
}