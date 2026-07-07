using RTG;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapVariantAdjustManager : Singleton<MapVariantAdjustManager> {

    public GameObject UIPrefab;

    List<MapVariant> variantCopies = new List<MapVariant>();
    MapVarAdjustUI _UIscript;
    GameObject _UIInstance;
    MapVariant _variantCopy;
    MapVariant _variantReference;

    public void EnterAdjusting(MapVariant mapToAdjust) {
        // Open UI
        ToggleUI(true);
        // Create gameobject copy of variant
        _variantReference = mapToAdjust;
        _variantCopy = SceneLoadingManager.Instance.InstantiateObjectInScene(mapToAdjust.gameObject, mapToAdjust.transform.position, mapToAdjust.transform.rotation)
            .GetComponent<MapVariant>();
        _variantCopy.gameObject.SetActive(true);
        // Make variant copies movable
        Movable movableReff = _variantCopy.AddComponent<Movable>();
        movableReff.ShownAxis = new List<GizmoAxis>() { GizmoAxis.X, GizmoAxis.Y, GizmoAxis.Z };
        movableReff.MovableType = GizmoType.Universal;
        GizmoManager.Instance.SetTargetGameObject(_variantCopy.gameObject);
        GizmoManager.Instance.ShowGizmo(GizmoType.Universal, new List<GizmoAxis> { GizmoAxis.All }, UniversalGizmoScaleDisabled: true);
        ObjectTransformGizmo.ObjectRestrictions restrictions = new ObjectTransformGizmo.ObjectRestrictions();
        GizmoManager.Instance.SetCustomRestrictions(
               MoveX: true,
               MoveY: true,
               MoveZ: true,
               CamRotationZ: true,
               CamRotationXY: true,
               RotationX: true,
               RotationY: true,
               RotationZ: true,
               Scale: false
        );
        // Add a color and transparency to the variant copies
        ChangeObjectMaterials(_variantCopy.gameObject, 0.8f, Color.magenta);
        // Show base map solid
        MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
    }

    public void ExitAdjusting(bool saveChanges = false) {
        // Apply position and rotation to the variants
        if (saveChanges) {
            Transform firstObj = MapManager.Instance.GetBaseMap().transform;
            Transform secondObj = _variantCopy.transform;

            Vector3 posOffset = firstObj.InverseTransformPoint(secondObj.position);
            Quaternion relQuad = Quaternion.Inverse(firstObj.rotation) * secondObj.rotation;
            Vector3 rotOffsetEuler = relQuad.eulerAngles;

            MapManager.Instance.ApplyAndSaveMapOffset(_variantReference, posOffset, rotOffsetEuler);
        }

        GizmoManager.Instance.HideGizmo();
        // Destroy all copies
        Destroy(_variantCopy.gameObject);
        // Hide UI
        ToggleUI(false);
        EditorManager.Instance.ChangeState(AppState.Freecam);
        MapManager.Instance.ToggleMapUI(true);
    }

    void ToggleUI(bool toggleOn) {
        if (_UIInstance == null && toggleOn) {
            _UIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(UIPrefab);
            _UIscript = _UIInstance.GetComponent<MapVarAdjustUI>();
        }
        _UIInstance.SetActive(toggleOn);
    }

    public void UpdatePosition(Vector3 newPosition) {
        if (_variantCopy != null) {
            _variantCopy.transform.position = newPosition;
        }
    }

    public void UpdateRotation(Vector3 newRotationEuler) {
        if (_variantCopy != null) {
            _variantCopy.transform.rotation = Quaternion.Euler(newRotationEuler);
        }
    }

    public Vector3 GetPosition() {
        return _variantCopy != null ? _variantCopy.transform.position : Vector3.zero;
    }

    public Vector3 GetRotationEuler() {
        return _variantCopy != null ? _variantCopy.transform.rotation.eulerAngles : Vector3.zero;
    }

    public GameObject GetCopiedVariant() {
        return _variantCopy.gameObject;
    }

    void ChangeObjectMaterials(GameObject targetObject, float transparency, Color newColor) {
        foreach (Transform child in targetObject.GetComponentsInChildren<Transform>()) {
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                child.AddComponent<MeshCollider>(); // add the collider
                Renderer rend = child.GetComponent<Renderer>();
                if (rend == null) {
                    print(child.name + "doesnt have renderer");
                    continue;
                }
                Material[] materials = rend.materials;
                foreach (Material mat in materials) {
                    Color color = newColor;
                    color.a = transparency;
                    mat.color = color;
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
            }
        }
    }
}
