using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuManager : MonoBehaviour { 
    public GameObject canvasPrefab;
    public GameObject contextMenuPrefab;
    public GameObject buttonPrefab;
    public GraphicRaycaster graphicRaycaster;

    void Awake() {
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
    }
}
