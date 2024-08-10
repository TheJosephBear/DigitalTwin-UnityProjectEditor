using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour {

    public string Name { get; private set; }
    public List<GameObject> Variants = new List<GameObject>();
    public GameObject selectedVariant;

    public void AddVariant(GameObject model) {
        Variants.Add(model);
        print(Variants.Count);
        foreach (GameObject variant in Variants) {
            print(variant.name);
        }
    }

    public void SetName(string name) {
        Name = name;
    }

    public GameObject Spawn(Vector3 pos) {
        GameObject spawned = Instantiate(Variants[0], pos, Quaternion.identity);
        spawned.SetActive(true);
        SpawnedDecoration script = spawned.AddComponent<SpawnedDecoration>();
        script.decorationPreset = this;
        script.decorationVariantIdx = 0;
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
