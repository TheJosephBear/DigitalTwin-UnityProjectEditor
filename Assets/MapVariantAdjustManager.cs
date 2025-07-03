using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapVariantAdjustManager : Singleton<MapVariantAdjustManager> {

    public GameObject UIreff; // temporary ui reff

    List<MapVariant> variantCopies = new List<MapVariant>();
    MapVarAdjustUI _UIscript;
    MapVariant _selectedVariant;

    public void EnterAdjusting() {
        // Open UI
        UIreff.SetActive(true);
        UIreff.GetComponent<UIBehaviour>().Show();
        _UIscript = FindAnyObjectByType<MapVarAdjustUI>();
        // Create gameobject copy of each variant
        print(MapManager.Instance.GetVariantsWithoutBase().Count);
        foreach (MapVariant original in MapManager.Instance.GetVariantsWithoutBase()) {
            MapVariant copy = Instantiate(original);
            copy.Name = original.Name;
            // Make variant copies movable
            Movable movableReff = copy.AddComponent<Movable>();
            movableReff.ShownAxis = new List<GizmoAxis>() { GizmoAxis.All };
            movableReff.MovableType = GizmoType.Universal;
            // Add a color and transparency to the variant copies
            ChangeObjectMaterials(copy.gameObject, 0.8f, Color.magenta);

            variantCopies.Add(copy);
        }
        // Load variant copies into UI dropdown
        _UIscript.FillDropdown(variantCopies);
        // Show base map solid
        MapManager.Instance.GetBaseMap().ToggleMeshVisibility(true);
        // Select and show the first variant in list
        SelectVariant(variantCopies[0]);
    }

    public void ExitAdjusting() {
        // Apply position and rotation to the variants

        // Destroy all copies

        // Hide UI
        UIreff.SetActive(false);
        UIreff.GetComponent<UIBehaviour>().Hide();
    }

    public void SelectVariant(MapVariant variant) {
        _selectedVariant?.gameObject.SetActive(false);
        _selectedVariant = variant;
        _selectedVariant.gameObject.SetActive(true);
    }

    public MapVariant GetSelectedVariant() {
        return _selectedVariant;
    }

    public void UpdatePosition(Vector3 newPosition) {
        if (_selectedVariant != null) {
            _selectedVariant.transform.position = newPosition;
        }
    }

    public void UpdateRotation(Vector3 newRotationEuler) {
        if (_selectedVariant != null) {
            _selectedVariant.transform.rotation = Quaternion.Euler(newRotationEuler);
        }
    }

    public Vector3 GetPosition() {
        return _selectedVariant != null ? _selectedVariant.transform.position : Vector3.zero;
    }

    public Vector3 GetRotationEuler() {
        return _selectedVariant != null ? _selectedVariant.transform.rotation.eulerAngles : Vector3.zero;
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
