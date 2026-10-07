using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiViewTester : MonoBehaviour {
    void Start() {
        EditorManager.Instance.MultiViewManager.EnterMultiView();
    }

}
