using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour {

    public string Name { get; private set; }
    List<GameObject> Variants = new List<GameObject>();

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

    public void Spawn(Vector3 pos) {
        GameObject spawned = Instantiate(Variants[0], pos, Quaternion.identity);
        spawned.SetActive(true); 
        AddMeshColliderToAllChildren(spawned);
    }

    void AddMeshColliderToAllChildren(GameObject g) {
        // Get all child transforms including the parent object itself
        foreach (Transform child in g.GetComponentsInChildren<Transform>()) {
            // Check if the child has a MeshRenderer or MeshFilter component
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                // Add MeshCollider if it doesn't already exist
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

}
