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

    protected override void Awake() {
        base.Awake();
    }

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
            _activeGizmoObject.Gizmo.SetEnabled(false);
        }
    }
    public bool IsShowingGizmo() {
        return _activeGizmoObject.Gizmo.IsEnabled;
    }

    public GameObject GetTargetObject() {
        return _targetObject;
    }

    public ObjectTransformGizmo GetActiveGizmo() {
        return _activeGizmoObject;
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

    public void SetCustomRestrictions(
        bool MoveX = true, 
        bool MoveY = true, 
        bool MoveZ = true,
        bool CamRotationXY = true,
        bool CamRotationZ = true,
        bool RotationX = true,
        bool RotationY = true,
        bool RotationZ = true,
        bool Scale = true
    ) {
        if (_targetObject == null || _activeGizmoObject == null) return;
        ObjectTransformGizmo.ObjectRestrictions restrictions = CreateNewRestrictionObject(_objectUniversalGizmo, _targetObject);

        // 2. Map your arguments to the exact axes/handles used in the original
        restrictions.SetCanMoveAlongAxis(0, MoveX);
        restrictions.SetCanMoveAlongAxis(1, MoveY);
        restrictions.SetCanMoveAlongAxis(2, MoveZ);

        restrictions.SetIsAffectedByHandle(GizmoHandleId.CamZRotation, CamRotationZ);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, CamRotationXY);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.XRotationSlider, RotationX);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.YRotationSlider, RotationY);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.ZRotationSlider, RotationZ);

        // 3. For scale, if 'Scale' is false, turn them all off like your original hardcoded version did
        restrictions.SetCanScaleAlongAxis(0, Scale);
        restrictions.SetCanScaleAlongAxis(1, Scale);
        restrictions.SetCanScaleAlongAxis(2, Scale);

        // 4. Register it back to the gizmo
        _objectUniversalGizmo.RegisterObjectRestrictions(_targetObject, restrictions);
    }

    private void ApplyRestrictions(GizmoType type, List<GizmoAxis> enabledAxisList) {
        if (_targetObject == null) return;

        ObjectTransformGizmo gizmo = null;
        switch (type) {
            case GizmoType.Position: gizmo = _objectMoveGizmo; break;
            case GizmoType.Rotation: gizmo = _objectRotationGizmo; break;
            case GizmoType.Scale: gizmo = _objectScaleGizmo; break;
            case GizmoType.Universal: gizmo = _objectUniversalGizmo; break;
        }

        if (gizmo == null) return;

        ObjectTransformGizmo.ObjectRestrictions restrictions = CreateNewRestrictionObject(gizmo, _targetObject);
        bool IsAxisEnabled(GizmoAxis axis) => enabledAxisList.Contains(axis) || enabledAxisList.Contains(GizmoAxis.All);

        bool x = IsAxisEnabled(GizmoAxis.X);
        bool y = IsAxisEnabled(GizmoAxis.Y);
        bool z = IsAxisEnabled(GizmoAxis.Z);

        // Pozice
        if (type == GizmoType.Position || type == GizmoType.Universal) {
            restrictions.SetCanMoveAlongAxis(0, x);
            restrictions.SetCanMoveAlongAxis(1, y);
            restrictions.SetCanMoveAlongAxis(2, z);
        }

        // Rotace
        if (type == GizmoType.Rotation || type == GizmoType.Universal) {
            restrictions.SetIsAffectedByHandle(GizmoHandleId.XRotationSlider, x);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.YRotationSlider, y);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.ZRotationSlider, z);

            // VypnutÌ "ball" rotace z vol·nÌ SetWorkGizmoId
            restrictions.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);
            restrictions.SetIsAffectedByHandle(GizmoHandleId.CamZRotation, false);
        }

        // äk·lov·nÌ
        if (type == GizmoType.Scale || type == GizmoType.Universal) {
            SetScaleAffected(restrictions, x, y, z);
        }

        gizmo.RegisterObjectRestrictions(_targetObject, restrictions);
    }

    // Pomocn· metoda pro korektnÌ vypnutÌ scale handl˘ v RTG
    private void SetScaleAffected(ObjectTransformGizmo.ObjectRestrictions restrictions, bool x, bool y, bool z) {
        // 1. MatematickÈ omezenÌ os (st·le platnÈ)
        restrictions.SetCanScaleAlongAxis(0, x);
        restrictions.SetCanScaleAlongAxis(1, y);
        restrictions.SetCanScaleAlongAxis(2, z);

        // 2. VypÌn·nÌ konkrÈtnÌch vizu·lnÌch handl˘ z tvÈ t¯Ìdy GizmoHandleId
        // KladnÈ osy (kostiËky na koncÌch os)
        restrictions.SetIsAffectedByHandle(GizmoHandleId.PXSlider, x);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.PYSlider, y);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.PZSlider, z);

        // Z·pornÈ osy (pokud je tvoje gizmo vykresluje do obou smÏr˘)
        restrictions.SetIsAffectedByHandle(GizmoHandleId.NXSlider, x);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.NYSlider, y);
        restrictions.SetIsAffectedByHandle(GizmoHandleId.NZSlider, z);

        // 3. St¯edov· kostka pro uniformnÌ ök·lov·nÌ (vöechny osy najednou)
        // Pokud je zak·zan· byù jen jedna osa, st¯edovÈ celkovÈ ök·lov·nÌ by mÏlo b˝t vypnutÈ
        if (!x || !y || !z) {
            restrictions.SetIsAffectedByHandle(GizmoHandleId.MidScaleCap, false);
        } else {
            restrictions.SetIsAffectedByHandle(GizmoHandleId.MidScaleCap, true);
        }
    }

    private void RestrictRotationBallHandle() {
        ObjectTransformGizmo.ObjectRestrictions restrictionsRot = CreateNewRestrictionObject(_objectRotationGizmo, _targetObject);
        ObjectTransformGizmo.ObjectRestrictions restrictionsUni = CreateNewRestrictionObject(_objectUniversalGizmo, _targetObject);
        restrictionsRot.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);
        restrictionsRot.SetIsAffectedByHandle(GizmoHandleId.CamZRotation, false);
        restrictionsUni.SetIsAffectedByHandle(GizmoHandleId.CamXYRotation, false);
        restrictionsUni.SetIsAffectedByHandle(GizmoHandleId.CamZRotation, false);
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