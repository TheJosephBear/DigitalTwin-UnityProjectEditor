using System.Collections;
using System.Collections.Generic;
using QuestionnaireToolkit.Scripts;
using UnityEngine;


namespace QuestionnaireToolkit {
    public interface IQuestionAdapter {
        public void SetQuestionText(string question);
        public string GetQuestionText();
        public void AddOption();
        public void SetOptionText(int optionIndex, string optionText);
        public void RemoveOption(int optionIndex);
        public List<QTOptionsData> GetOptionsData();
        public int GetOptionsCount();


    }
}