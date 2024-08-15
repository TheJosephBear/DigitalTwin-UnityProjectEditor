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
    public List<ModelAsset> Variants = new List<ModelAsset>();

    public void AddVariant(ModelAsset model) {
        if(Variants.Contains(model)) return;
        Variants.Add(model);
    }

    public void SetName(string name) {
        Name = name;
    }

    public GameObject Spawn(Vector3 pos, int variantIdx = 0) {
        GameObject spawned = Variants[variantIdx].InstantiateModel(pos);
        spawned.SetActive(true);
        Decoration decoScript = spawned.AddComponent<Decoration>();
        decoScript.decorationPreset = this;
        decoScript.decorationVariantIdx = variantIdx;
        /*
        print("testin");
        print(this);
        print(this.Name);
        print(decoScript.decorationPreset);
        */
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

[Serializable]
public class SerializableDecorationPreset {
    public string name;
    public List<string> modelAssetIDs;  
}