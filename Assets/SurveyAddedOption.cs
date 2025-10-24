using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurveyAddedOption : MonoBehaviour
{
    public TEMPQuestionnareSetUpUI _rootUIReff;
    int _optionIndex = 0;

    public TMP_InputField InputFieldReff;

    public void RemoveOption() {
        _rootUIReff.RemoveOption(_optionIndex);
    }

    public void Initialize(string name, int index, TEMPQuestionnareSetUpUI rootUIReff) {
        InputFieldReff.text = name;
        _optionIndex = index;
        _rootUIReff = rootUIReff;
    }

    public void ChangeOptionName() {
        _rootUIReff.SetOptionText(_optionIndex, InputFieldReff.text);
    }


}
