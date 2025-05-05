using Cinemachine;
using RTG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoManager : Singleton<GizmoManager> {

    private GameObject targetObject;
    private ObjectTransformGizmo transformGizmo = null;

    public void SetTargetGameObject(GameObject go) {
        targetObject = go;
    }

    public void HideGizmo() {
        if (transformGizmo != null) {
            RTGizmosEngine.Get.RemoveGizmo(transformGizmo.Gizmo);
            transformGizmo = null;
        }
    }

    public void ShowGizmo(MovableType type, GizmoAxis axis = GizmoAxis.All) {
        HideGizmo();

        switch (type) {
            case MovableType.Universal:
                transformGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();
                ApplyUniversalAxisConstraint(transformGizmo, targetObject, axis);
                break;

            case MovableType.Position:
                transformGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
                ApplyPositionAxisConstraint(transformGizmo, targetObject, axis);
                break;

            case MovableType.Rotation:
                transformGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
                ApplyRotationAxisConstraint(transformGizmo.Gizmo.RotationGizmo, transformGizmo, targetObject, axis);
                break;

            case MovableType.Scale:
                transformGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
                ApplyScaleAxisConstraint(transformGizmo, targetObject, axis);
                break;
        }

        transformGizmo.SetTargetObject(targetObject);
        transformGizmo.SetTransformSpace(GizmoSpace.Local);
    }

    public void ShowGizmo(List<MovableType> types, List<GizmoAxis> axes) {
        HideGizmo();

        if (types == null || axes == null || types.Count == 0 || axes.Count == 0) {
            Debug.LogWarning("No MovableType or GizmoAxis selected.");
            return;
        }

        GizmoAxis axisFlags = GizmoAxis.None;
        foreach (var ax in axes)
            axisFlags |= ax;

        foreach (var type in types) {
            ObjectTransformGizmo gizmo = null;

            switch (type) {
                case MovableType.Position:
                    gizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
                    ApplyPositionAxisConstraint(gizmo, targetObject, axisFlags);
                    break;

                case MovableType.Rotation:
                    gizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
                    ApplyRotationAxisConstraint(gizmo.Gizmo.RotationGizmo, gizmo, targetObject, axisFlags);
                    break;

                case MovableType.Scale:
                    gizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
                    ApplyScaleAxisConstraint(gizmo, targetObject, axisFlags);
                    break;

                case MovableType.Universal:
                    gizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();
                    ApplyUniversalAxisConstraint(gizmo, targetObject, axisFlags);
                    break;
            }

            if (gizmo != null) {
                gizmo.SetTargetObject(targetObject);
                gizmo.SetTransformSpace(GizmoSpace.Local);
                transformGizmo = gizmo;
            }
        }
    }


    public bool IsShowingGizmo() {
        return transformGizmo != null;
    }

    #region Constraint functions

    private void ApplyUniversalAxisConstraint(ObjectTransformGizmo universalGizmo, GameObject target, GizmoAxis axis) {
        var restrictions = new ObjectTransformGizmo.ObjectRestrictions();

        // Position
        for (int i = 0; i < 3; i++)
            restrictions.SetCanMoveAlongAxis(i, axis == GizmoAxis.All || i == (int)axis);

        // Rotation
        if (axis != GizmoAxis.X) restrictions.SetIsAffectedByHandle(GizmoHandleId.XRotationSlider, false);
        if (axis != GizmoAxis.Y) restrictions.SetIsAffectedByHandle(GizmoHandleId.YRotationSlider, false);
        if (axis != GizmoAxis.Z) restrictions.SetIsAffectedByHandle(GizmoHandleId.ZRotationSlider, false);
        if (axis != GizmoAxis.All) restrictions.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);

        // Scale
        for (int i = 0; i < 3; i++)
            restrictions.SetCanScaleAlongAxis(i, axis == GizmoAxis.All || i == (int)axis);

        universalGizmo.RegisterObjectRestrictions(target, restrictions);
    }

    private void ApplyPositionAxisConstraint(ObjectTransformGizmo moveGizmo, GameObject target, GizmoAxis axis) {
        var restrictions = new ObjectTransformGizmo.ObjectRestrictions();
        for (int i = 0; i < 3; i++)
            restrictions.SetCanMoveAlongAxis(i, axis == GizmoAxis.All || i == (int)axis);

        moveGizmo.RegisterObjectRestrictions(target, restrictions);
    }

    private void ApplyRotationAxisConstraint(RotationGizmo rotationGizmo, ObjectTransformGizmo objectTransformGizmo, GameObject target, GizmoAxis axis) {
        var restrictions = new ObjectTransformGizmo.ObjectRestrictions();

        if (axis != GizmoAxis.X) restrictions.SetIsAffectedByHandle(GizmoHandleId.XRotationSlider, false);
        if (axis != GizmoAxis.Y) restrictions.SetIsAffectedByHandle(GizmoHandleId.YRotationSlider, false);
        if (axis != GizmoAxis.Z) restrictions.SetIsAffectedByHandle(GizmoHandleId.ZRotationSlider, false);
        if (axis != GizmoAxis.All) restrictions.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);

        objectTransformGizmo.RegisterObjectRestrictions(target, restrictions);

        // Also visually disable
        foreach (var (ax, id) in new Dictionary<GizmoAxis, int>
        {
        { GizmoAxis.X, GizmoHandleId.XRotationSlider },
        { GizmoAxis.Y, GizmoHandleId.YRotationSlider },
        { GizmoAxis.Z, GizmoHandleId.ZRotationSlider }
    }) {
            var handle = rotationGizmo.Gizmo.GetHandleById_SystemCall(id);
            bool show = axis == GizmoAxis.All || ax == axis;
            handle.SetHoverable(show);
            handle.SetVisible(show);
        }

        var camXYHandle = rotationGizmo.Gizmo.GetHandleById_SystemCall(GizmoHandleId.CamXYRotation);
        camXYHandle.SetHoverable(axis == GizmoAxis.All);
        camXYHandle.SetVisible(axis == GizmoAxis.All);
    }

    private void ApplyScaleAxisConstraint(ObjectTransformGizmo scaleGizmo, GameObject target, GizmoAxis axis) {
        var restrictions = new ObjectTransformGizmo.ObjectRestrictions();
        for (int i = 0; i < 3; i++)
            restrictions.SetCanScaleAlongAxis(i, axis == GizmoAxis.All || i == (int)axis);

        scaleGizmo.RegisterObjectRestrictions(target, restrictions);
    }

    #endregion


    /* If i want the camera to move around while still using the cinemachine... 
     * Otherwise i have to turn off the cinemachine brain when in default editor state 
     */

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

public enum MovableType {
    Rotation,
    Position,
    Scale,
    Universal,
}

public enum GizmoAxis {
    X,
    Y,
    Z,
    All,
    None,
}