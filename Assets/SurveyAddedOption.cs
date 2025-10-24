using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurveyAddedOption : MonoBehaviour
{

    public int OptionIndex = 0;

    public TMP_InputField InputFieldReff;

    public void RemoveOption() {

    }

    public void SetOptionName(string name) {
        InputFieldReff.text = name;
    }


}
