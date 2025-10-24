using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QuestionnaireToolkit {
    public interface IQuestionAdapter {
        public void SetQuestionText(string question);
        public void AddOption();
        public void SetOptionText(int optionIndex, string optionText);
        public void RemoveOption(int optionIndex);
        public int GetOptionsCount();

    }
}