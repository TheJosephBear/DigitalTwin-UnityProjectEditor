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
            
            ObjectTransformGizmo.ObjectRestrictions restrictions = new ObjectTransformGizmo.ObjectRestrictions();
            /*
            restrictions.SetCanMoveAlongAxis(0, true);
            restrictions.SetCanMoveAlongAxis(1, true);
            restrictions.SetCanMoveAlongAxis(2, false);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.CamZRotation, true);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.CamZRotation, false);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.XRotationSlider, false);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.YRotationSlider, false);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.ZRotationSlider, false);
            restrictions.SetCanScaleAlongAxis(0, false);
            restrictions.SetCanScaleAlongAxis(1, false);
            restrictions.SetCanScaleAlongAxis(2, false);
            */
            gizmoManager.SetCustomRestrictions(restrictions);
        }
    }
}
