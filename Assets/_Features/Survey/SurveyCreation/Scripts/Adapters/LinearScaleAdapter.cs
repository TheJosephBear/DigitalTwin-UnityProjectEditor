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
            _linearScale.DeleteItem(GetOptionsCount()-1, optionIndex);
            _linearScale.OnValidate();
        }

        public int GetOptionsCount() {
            return _linearScale.options.Count;
        }
    }
}