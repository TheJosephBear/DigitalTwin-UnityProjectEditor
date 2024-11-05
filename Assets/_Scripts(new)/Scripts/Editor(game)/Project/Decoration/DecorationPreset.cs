using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationPreset {
    /// <summary>
    /// Decoration type (tree, bush, bench, ...)
    /// Can have multiple variants and have multiple instances in scene (The instances themselves are Decoration class that refferences this class)
    /// </summary>
    
    public string Name { get; private set; }
    public List<DecorationVariant> Variants = new List<DecorationVariant>();

    public void AddVariant(string name, ModelAsset model) {
        Variants.Add(new DecorationVariant(name, model));
    }

    public void SetName(string name) {
        Name = name;
    }

    public GameObject Spawn(Vector3 pos, DecorationVariant variant) {
        GameObject spawned = variant.Model.InstantiateModel(pos);
        spawned.SetActive(true);
        DecorationInstantiated decoScript = spawned.AddComponent<DecorationInstantiated>();
        decoScript.decorationPreset = this;
        decoScript.decorationVariant = variant;
        decoScript.SetName(variant.Name);
        AddMeshColliderToAllChildren(spawned);
        return spawned;
    }

    void AddMeshColliderToAllChildren(GameObject g) {
        foreach (Transform child in g.GetComponentsInChildren<Transform>()) {
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                if (child.GetComponent<MeshCollider>() == null) {
                    child.gameObject.AddComponent<MeshCollider>();
                    ChangeLayer(child.gameObject, "Movable");
                }
            }
        }
    }

    void ChangeLayer(GameObject obj, string layerName) {
        obj.layer = LayerMask.NameToLayer(layerName);
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null) {
            collider.enabled = false;
            collider.enabled = true;
        }
    }

    public void Serialize() {

    }

}