using Cinemachine;
using RTG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoManager : Singleton<GizmoManager> {

    private ObjectTransformGizmo _objectMoveGizmo;
    private ObjectTransformGizmo _objectRotationGizmo;
    private ObjectTransformGizmo _objectScaleGizmo;
    private ObjectTransformGizmo _objectUniversalGizmo;

    private GizmoType _activeGizmoType;
    /// <summary>
    /// A reference to the current work gizmo. If the work gizmo id is GizmoId.Move, then
    /// this will point to '_objectMoveGizmo'. For GizmoId.Rotate, it will point to 
    /// '_objectRotationGizmo' and so on.
    /// </summary>
    private ObjectTransformGizmo _activeGizmoObject;
    /// <summary>
    /// A reference to the target object. This is the object that will be manipulated by
    /// the gizmos and it will always be picked from the scene via a mouse click. This will
    /// be set to null when the user clicks in thin air.
    /// </summary>
    private GameObject _targetObject;

    private void Start() {
        // Create the 4 gizmos
        _objectMoveGizmo = RTGizmosEngine.Get.CreateObjectMoveGizmo();
        _objectRotationGizmo = RTGizmosEngine.Get.CreateObjectRotationGizmo();
        _objectScaleGizmo = RTGizmosEngine.Get.CreateObjectScaleGizmo();
        _objectUniversalGizmo = RTGizmosEngine.Get.CreateObjectUniversalGizmo();

        // Call the 'SetEnabled' function on the parent gizmo to make sure
        // the gizmos are initially hidden in the scene. We want the gizmo
        // to show only when we have a target object available.
        _objectMoveGizmo.Gizmo.SetEnabled(false);
        _objectRotationGizmo.Gizmo.SetEnabled(false);
        _objectScaleGizmo.Gizmo.SetEnabled(false);
        _objectUniversalGizmo.Gizmo.SetEnabled(false);

        // We initialize the work gizmo to the move gizmo by default. This means
        // that the first time an object is clicked, the move gizmo will appear.
        // You can change the default gizmo, by simply changing these 2 lines of
        // code. For example, if you wanted the scale gizmo to be the default work
        // gizmo, replace '_objectMoveGizmo' with '_objectScaleGizmo' and GizmoId.Move
        // with GizmoId.Scale.
        _activeGizmoObject = _objectMoveGizmo;
        _activeGizmoType = GizmoType.Position;
    }

    #region public functions

    public void SetTargetGameObject(GameObject go) {
        OnTargetObjectChanged(go);
    }

    public void ShowGizmo(GizmoType type, GizmoAxis axis = GizmoAxis.All, bool UniversalGizmoScaleDisabled = false) {
        SetWorkGizmoId(type);
        if (UniversalGizmoScaleDisabled) {
            RestrictUniversalScaleHandle();
        }
    }

    public void ShowGizmo(GizmoType type, List<GizmoAxis> enabledAxisList, bool UniversalGizmoScaleDisabled = false) {
        SetWorkGizmoId(type);
        if (UniversalGizmoScaleDisabled) {
            RestrictUniversalScaleHandle();
        }
        ApplyRestrictions(type, enabledAxisList);
    }

    public void HideGizmo() {
        if (_activeGizmoObject != null) {
            RTGizmosEngine.Get.RemoveGizmo(_activeGizmoObject.Gizmo);
            _activeGizmoObject = null;
        }
    }
    public bool IsShowingGizmo() {
        return _activeGizmoObject != null;
    }

    #endregion


    /// <summary>
    /// This function is called to change the type of work gizmo.
    /// </summary>
    private void SetWorkGizmoId(GizmoType gizmoId) {
        // Start with a clean slate and disable all gizmos
        _objectMoveGizmo.Gizmo.SetEnabled(false);
        _objectRotationGizmo.Gizmo.SetEnabled(false);
        _objectScaleGizmo.Gizmo.SetEnabled(false);
        _objectUniversalGizmo.Gizmo.SetEnabled(false);

        _activeGizmoType = gizmoId;
        switch (gizmoId) {
            case GizmoType.Position:
                _activeGizmoObject = _objectMoveGizmo;
                break;
            case GizmoType.Rotation:
                _activeGizmoObject = _objectRotationGizmo;
                // Strange rotation outside the classic handles - disable it
                RestrictRotationBallHandle();
                break;
            case GizmoType.Scale:
                _activeGizmoObject = _objectScaleGizmo;
                break;
            case GizmoType.Universal:
                _activeGizmoObject = _objectUniversalGizmo;
                // Strange rotation outside the classic handles - disable it
                RestrictRotationBallHandle();
                break;
        }

        if (_targetObject != null) _activeGizmoObject.Gizmo.SetEnabled(true);
    }

    /// <summary>
    /// Called from when the user clicks on a game object
    /// that is different from the current target object. The function takes care
    /// of adjusting the gizmo states accordingly.
    /// </summary>
    private void OnTargetObjectChanged(GameObject newTargetObject) {
        // Store the new target object
        _targetObject = newTargetObject;

        // Is the target object valid?
        if (_targetObject != null) {
            // It is. Now call 'SetTargetObject' for all gizmos. After the next 4 lines
            // of code are executed, all gizmos will be able to control this object.
            _objectMoveGizmo.SetTargetObject(_targetObject);
            _objectRotationGizmo.SetTargetObject(_targetObject);
            _objectScaleGizmo.SetTargetObject(_targetObject);
            _objectUniversalGizmo.SetTargetObject(_targetObject);

            // Make sure the work gizmo is enabled. We always activate the work gizmo when
            // a target object is valid. There is no need to check if the gizmo is already
            // enabled. The 'SetEnabled' call will simply be ignored if that is the case.
            _activeGizmoObject.Gizmo.SetEnabled(true);
        } else {
            // The target object is null. In this case, we don't want any gizmos to be visible
            // in the scene, so we disable all of them.
            _objectMoveGizmo.Gizmo.SetEnabled(false);
            _objectRotationGizmo.Gizmo.SetEnabled(false);
            _objectScaleGizmo.Gizmo.SetEnabled(false);
            _objectUniversalGizmo.Gizmo.SetEnabled(false);
        }
    }

    #region Gizmo restriction functions

    private void ApplyRestrictions(GizmoType type, List<GizmoAxis> enabledAxisList) {
        if (_targetObject == null) return;

        ObjectTransformGizmo gizmo = null;

        switch (type) {
            case GizmoType.Position:
                gizmo = _objectMoveGizmo;
                break;
            case GizmoType.Rotation:
                gizmo = _objectRotationGizmo;
                break;
            case GizmoType.Scale:
                gizmo = _objectScaleGizmo;
                break;
            case GizmoType.Universal:
                gizmo = _objectUniversalGizmo;
                break;
        }

        if (gizmo == null) return;

        ObjectTransformGizmo.ObjectRestrictions restrictions = CreateNewRestrictionObject(gizmo, _targetObject);

        // Helper for checking if an axis is enabled
        bool IsAxisEnabled(GizmoAxis axis) => enabledAxisList.Contains(axis) || enabledAxisList.Contains(GizmoAxis.All);

        // Position and Scale: Disable movement/scale along each axis
        if (type == GizmoType.Position || type == GizmoType.Universal) {
            restrictions.SetCanMoveAlongAxis(0, IsAxisEnabled(GizmoAxis.X));
            restrictions.SetCanMoveAlongAxis(1, IsAxisEnabled(GizmoAxis.Y));
            restrictions.SetCanMoveAlongAxis(2, IsAxisEnabled(GizmoAxis.Z));
        }

        if (type == GizmoType.Scale || type == GizmoType.Universal) {
            restrictions.SetCanScaleAlongAxis(0, IsAxisEnabled(GizmoAxis.X));
            restrictions.SetCanScaleAlongAxis(1, IsAxisEnabled(GizmoAxis.Y));
            restrictions.SetCanScaleAlongAxis(2, IsAxisEnabled(GizmoAxis.Z));
        }

        if (type == GizmoType.Rotation || type == GizmoType.Universal) {
            restrictions.SetIsAffectedByHandle(GizmoHandleId.XRotationSlider, IsAxisEnabled(GizmoAxis.X));
            restrictions.SetIsAffectedByHandle(GizmoHandleId.YRotationSlider, IsAxisEnabled(GizmoAxis.Y));
            restrictions.SetIsAffectedByHandle(GizmoHandleId.ZRotationSlider, IsAxisEnabled(GizmoAxis.Z));
        }

        gizmo.RegisterObjectRestrictions(_targetObject, restrictions);
    }


    private void RestrictRotationBallHandle() {
        ObjectTransformGizmo.ObjectRestrictions restrictionsRot = CreateNewRestrictionObject(_objectRotationGizmo, _targetObject);
        ObjectTransformGizmo.ObjectRestrictions restrictionsUni = CreateNewRestrictionObject(_objectUniversalGizmo, _targetObject);
        restrictionsRot.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);
        restrictionsUni.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);
        _objectRotationGizmo.RegisterObjectRestrictions(_targetObject, restrictionsRot);
        _objectUniversalGizmo.RegisterObjectRestrictions(_targetObject, restrictionsUni);
    }

    private void RestrictUniversalScaleHandle() {
        ObjectTransformGizmo.ObjectRestrictions restrictions = CreateNewRestrictionObject(_objectUniversalGizmo, _targetObject);
        restrictions.SetCanScaleAlongAxis(0, false);
        restrictions.SetCanScaleAlongAxis(1, false);
        restrictions.SetCanScaleAlongAxis(2, false);
        _objectUniversalGizmo.RegisterObjectRestrictions(_targetObject, restrictions);
    }

    private ObjectTransformGizmo.ObjectRestrictions CreateNewRestrictionObject(ObjectTransformGizmo gizmoGameObject, GameObject targetGameObject) {
        if (gizmoGameObject == null) {
            print("gizmo object is null!");
            return null;
        }
        if (targetGameObject == null) {
            print("_targetObject is null!");
            return null;
        }
        ObjectTransformGizmo.ObjectRestrictions restrictions = new ObjectTransformGizmo.ObjectRestrictions();
        ObjectTransformGizmo.ObjectRestrictions originalRestrictions = gizmoGameObject.GetObjectRestrictions(targetGameObject); ;
        if (originalRestrictions != null) restrictions = originalRestrictions;
        return restrictions;
    }
    #endregion
}

public enum GizmoType {
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