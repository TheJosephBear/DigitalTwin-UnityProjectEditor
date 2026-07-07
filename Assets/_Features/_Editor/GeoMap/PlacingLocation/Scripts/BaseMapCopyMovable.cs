using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;

public class BaseMapCopyMovable : Movable {
    public override void OnClickDown() {
        print("Special click down");
        GizmoManager gizmoManager = GizmoManager.Instance;
        if (gizmoManager.GetTargetObject() != this && !gizmoManager.IsShowingGizmo()) {
            gizmoManager.SetTargetGameObject(gameObject);
            gizmoManager.ShowGizmo(MovableType, ShownAxis);

            // Special restrictions
            
         //   ObjectTransformGizmo.ObjectRestrictions restrictions = new ObjectTransformGizmo.ObjectRestrictions();

            gizmoManager.SetCustomRestrictions(
                MoveX: true,
                MoveY: false,
                MoveZ: true,
                CamRotationZ: true,
                CamRotationXY: false,
                RotationX: false,
                RotationY: false,
                RotationZ: false,
                Scale: false
            /*restrictions*/);
        }
    }
}
