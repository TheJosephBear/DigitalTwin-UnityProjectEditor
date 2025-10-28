using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionnareTester : MonoBehaviour
{
    void Awake(){
        ViewManager.Instance.CreateNewViewPoint();
        ViewManager.Instance.CreateNewViewPoint();
        ViewManager.Instance.CreateNewViewPoint();
        ViewManager.Instance.CreateNewViewPoint();
    }
}
