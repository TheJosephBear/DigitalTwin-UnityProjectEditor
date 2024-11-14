using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrolviewAlwaysLastItem : MonoBehaviour {

    public Transform ScrollViewContent;
    void Update() {
        transform.SetSiblingIndex(ScrollViewContent.childCount - 1);
    }

}
