using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMovableObjects : MonoBehaviour {
    /// <summary>
    /// This is the logic for clicking objects in scene in orther to change their transform
    /// Transform manipulation is handled by GizmoController
    /// </summary>
     
  //  GizmoController gizmoController;
    GameObject selectedObject;
    bool canMove = true;

    void Awake() {
     //   gizmoController = GizmoController.Instance;
    }

    void Update() {
        if (!canMove)
            return;
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
                        // if its not a pivot!!!
                        if(hitObject.layer != LayerMask.NameToLayer("GizmoPivot")){
                            DeselectObject();
                        }
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
   //     gizmoController.SelectGameObject(selectedObject);
    }

    void DeselectObject() {
        selectedObject = null;
    //    gizmoController.DeselectGameObject();
    }

    public void ToggleMoveEnable() {
        canMove = !canMove;
    }


}
