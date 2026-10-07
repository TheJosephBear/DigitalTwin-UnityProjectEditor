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
        if (mapToAdjust == null || mapToAdjust.ModelAsset == null) {
            Debug.LogError("Cannot adjust map variant: Target or ModelAsset is null.");
            return;
        }

        ToggleUI(true);
        _variantReference = mapToAdjust;

        // 1. Hide the original model while adjusting to prevent z-fighting and asset conflicts
        _variantReference.gameObject.SetActive(false);

        // 2. Instantiate a FRESH visual model from the source ModelAsset
        // (This avoids duplicating active state/scripts that mess up the original)
        GameObject previewGo = _variantReference.ModelAsset.InstantiateModel(_variantReference.transform.position);
        previewGo.transform.rotation = _variantReference.transform.rotation;
        previewGo.SetActive(true);

        // 3. Attach a fresh MapVariant component to the clone, NOT the old one
        _variantCopy = previewGo.AddComponent<MapVariant>();
        _variantCopy.ModelAsset = _variantReference.ModelAsset;
        _variantCopy.Name = _variantReference.Name + "_Preview";

        // 4. Attach movement gizmo setup
        Movable movableReff = previewGo.AddComponent<Movable>();
        movableReff.ShownAxis = new List<GizmoAxis>() { GizmoAxis.X, GizmoAxis.Y, GizmoAxis.Z };
        movableReff.MovableType = GizmoType.Universal;

        GizmoManager.Instance.SetTargetGameObject(previewGo);
        GizmoManager.Instance.ShowGizmo(GizmoType.Universal, new List<GizmoAxis> { GizmoAxis.All }, UniversalGizmoScaleDisabled: true);

        GizmoManager.Instance.SetCustomRestrictions(
            MoveX: true, MoveY: true, MoveZ: true,
            CamRotationZ: true, CamRotationXY: true,
            RotationX: true, RotationY: true, RotationZ: true,
            Scale: false
        );

        // 5. Apply preview materials safely
        ApplyPreviewMaterial(previewGo, new Color(1f, 0f, 1f, 0.5f));

        // Show base map solid if needed
        if (MapManager.Instance.GetBaseMap() != null) {
            MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
        }
    }

    public void ExitAdjusting(bool saveChanges = false) {
        if (saveChanges && _variantCopy != null && _variantReference != null) {
            Transform copyTransform = _variantCopy.transform;

            MapManager.Instance.ApplyAndSaveMapTransform(
                _variantReference,
                copyTransform.position,
                copyTransform.rotation.eulerAngles
            );
        }

        GizmoManager.Instance.HideGizmo();

        // Destroy ONLY the temporary adjustment copy
        if (_variantCopy != null) {
            Destroy(_variantCopy.gameObject);
            _variantCopy = null;
        }

        // Re-enable and restore the original target map model
        if (_variantReference != null) {
            _variantReference.gameObject.SetActive(true);
            _variantReference.ToggleMeshVisibility(true);
            _variantReference = null;
        }

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

    private void ApplyPreviewMaterial(GameObject targetObject, Color color) {
        // Create a single transparent preview material instance
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null) standardShader = Shader.Find("Universal Render Pipeline/Lit");

        Material previewMat = new Material(standardShader);
        previewMat.color = color;

        // Set transparency modes
        previewMat.SetFloat("_Mode", 3); // Transparent
        previewMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        previewMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        previewMat.SetInt("_ZWrite", 0);
        previewMat.DisableKeyword("_ALPHATEST_ON");
        previewMat.EnableKeyword("_ALPHABLEND_ON");
        previewMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        previewMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Get all renderers on the target preview
        MeshRenderer[] renderers = targetObject.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer rend in renderers) {
            // Ensure a MeshCollider exists for gizmo raycasting on the clone
            if (rend.GetComponent<MeshCollider>() == null && rend.GetComponent<MeshFilter>() != null) {
                MeshCollider col = rend.gameObject.AddComponent<MeshCollider>();
                col.sharedMesh = rend.GetComponent<MeshFilter>().sharedMesh;
            }

            // Override materials with our temporary material instance array
            Material[] mats = new Material[rend.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++) {
                mats[i] = previewMat;
            }

            rend.materials = mats;
        }
    }

}
