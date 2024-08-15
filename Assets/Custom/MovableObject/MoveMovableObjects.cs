using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMovableObjects : MonoBehaviour {

    public GameObject pivotArrows;
    GameObject selectedObject;
    GameObject selectedObjectArrows;


    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                GameObject hitObject = hit.collider.gameObject;
                print(hitObject.layer);
                if (selectedObject == null) {
                    if (hitObject.layer == LayerMask.NameToLayer("Movable")) {
                        SelectObject(hitObject);
                    }
                } else {
                    if (hitObject.transform.root.gameObject != selectedObject) {
                        DeselectObject();
                        if (hitObject.layer == LayerMask.NameToLayer("Movable")) {
                            SelectObject(hitObject);
                        }
                    }
                }
            }
        }
    }

    void SelectObject(GameObject obj) {
        selectedObject = obj.transform.root.gameObject;
        StartMovingObject();
    }

    void DeselectObject() {
        selectedObject = null;
        Destroy(selectedObjectArrows);
        selectedObjectArrows = null;
    }

    public void StartMovingObject() {
        selectedObjectArrows = Instantiate(pivotArrows, selectedObject.transform);
    }
}
