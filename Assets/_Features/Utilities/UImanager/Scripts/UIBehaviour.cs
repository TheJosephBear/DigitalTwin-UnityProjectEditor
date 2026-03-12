using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
public abstract class UIBehaviour : MonoBehaviour {

    Canvas canvas;
    GraphicRaycaster raycaster;
    bool isSetup = false;

    protected virtual void Awake() {
        canvas = GetComponent<Canvas>();
        raycaster = GetComponent<GraphicRaycaster>();
        isSetup = true;
    }

    public virtual void Show() {
        canvas.enabled = true;
        raycaster.enabled = true;
    }

    public virtual void Hide() {
        canvas.enabled = false;
        raycaster.enabled = false;

    }

    public bool IsSetup() {
        return isSetup;
    }
}
