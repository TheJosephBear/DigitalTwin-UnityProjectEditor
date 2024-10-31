using RTG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoManager : Singleton<GizmoManager> {

    GameObject targetObject;
    ObjectTransformGizmo transformGizmo = null;

    public void SetTargetGameObject(GameObject go) {
        targetObject = go;
    }

    public void ShowUniversalGizmo() {
        if (transformGizmo==null) transformGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        transformGizmo.SetTargetObject(targetObject);
        transformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { targetObject });
        transformGizmo.SetTransformSpace(GizmoSpace.Local);
    }

    public void ShowPositionGizmo() {
        if (transformGizmo == null) transformGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();

        transformGizmo.SetTargetObject(targetObject);
        transformGizmo.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { targetObject });
        transformGizmo.SetTransformSpace(GizmoSpace.Local);
    }

    public void ShowRotationGizmo() {
        if (transformGizmo == null) transformGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();

        transformGizmo.SetTargetObject(targetObject);
        transformGizmo.SetTransformSpace(GizmoSpace.Local);
    }

    public void ShowScaleGizmo() {
        if (transformGizmo == null) transformGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();

        transformGizmo.SetTargetObject(targetObject);
        transformGizmo.SetTransformSpace(GizmoSpace.Local);
    }

    /*foreach (var targetName in moveTargetNames)
            {
                var transformGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.Gizmo.MoveGizmo.SetVertexSnapTargetObjects(new List<GameObject> { targetObject });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }
            
            var rotationTargetNames = new string[] { "Cylinder", "Red Cube" };
            foreach (var targetName in rotationTargetNames)
            {
                var transformGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }

            var scaleTargetNames = new string[] { "Cylinder (1)", "Sphere (1)" };
            foreach (var targetName in scaleTargetNames)
            {
                var transformGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }

            var universalTargetNames = new string[] { "Blue Cube (1)", "Green Cube" };
            foreach (var targetName in universalTargetNames)
            {
                var transformGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

                GameObject targetObject = GameObject.Find(targetName);
                transformGizmo.SetTargetObject(targetObject);
                transformGizmo.Gizmo.UniversalGizmo.SetMvVertexSnapTargetObjects(new List<GameObject> { targetObject });
                transformGizmo.SetTransformSpace(GizmoSpace.Local);
            }*/

}