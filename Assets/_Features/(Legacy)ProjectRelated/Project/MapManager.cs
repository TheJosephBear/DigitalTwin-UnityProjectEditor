using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;
using static UnityEngine.InputSystem.Android.LowLevel.AndroidGameControllerState;

public class MapManager : Singleton<MapManager> {

    ModelAsset baseMap;
    List<ModelAsset> mapVariants = new List<ModelAsset>();
    public Vector3 mapSpawnPosition;

    GameObject currentMapVarInstance;

    protected override void Awake() {
        base.Awake();
    }

    public void UploadBaseMapModel(ModelAsset newMap) {
        baseMap = newMap;
        SpawnMap();
    }

    public void UploadMapVariant(ModelAsset newMap) {
        mapVariants.Add(newMap);
    }

    public void SpawnMap() {
        GameObject go = baseMap?.InstantiateModel(mapSpawnPosition);
        go?.SetActive(true);
    }

    public void SpawnSelectedVariant(int index) {
        if (currentMapVarInstance != null) {
            Destroy(currentMapVarInstance);
            currentMapVarInstance = null;
        }
        if (index >= 0 && index < mapVariants.Count) {
            currentMapVarInstance = mapVariants[index]?.InstantiateModel(mapSpawnPosition);
            currentMapVarInstance?.SetActive(true);
            AddLayerToAllChildren(currentMapVarInstance);
        }
    }


    void AddLayerToAllChildren(GameObject g) {
        foreach (Transform child in g.GetComponentsInChildren<Transform>()) {
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                if (child.GetComponent<MeshCollider>() == null) {
                    child.gameObject.layer = LayerMask.NameToLayer("SecondaryMap");
                }
            }
        }
    }

    public void ClearEverything() {
        baseMap = null;
    }

    public bool hasVariant() {
        return mapVariants.Count > 0;
    }

    public List<ModelAsset> GetVariants() {
        return mapVariants;
    }


    public SerializableMap Serialize() {
        if (baseMap == null) {
            return null;
        }

        List<SerializableMapVariant> variantListSerialized = new List<SerializableMapVariant>();

        foreach (var variant in mapVariants) {
            variantListSerialized.Add(new SerializableMapVariant {
                modelID = variant.ModelID
            });
        }

        SerializableMap serializedMap = new SerializableMap {
            baseModelID = baseMap.ModelID,
            variants = variantListSerialized
        };

        return serializedMap;
    }
    public void Deserialize(SerializableMap serializedMap) {
        if (serializedMap == null) return;

        // Deserialize base map
        ModelAsset mapAsset = AssetManager.Instance.FindModelAssetByID(serializedMap.baseModelID);
        UploadBaseMapModel(mapAsset);
        SpawnMap();

        // Deserialize variants
        foreach (var variant in serializedMap.variants) {
            ModelAsset modelAsset = AssetManager.Instance.FindModelAssetByID(variant.modelID);
            UploadMapVariant(modelAsset);
        }
    }

}

[Serializable]
public class SerializableMap {
    public string baseModelID; // base map
    public List<SerializableMapVariant> variants;
}

[Serializable]
public class SerializableMapVariant {
    public string modelID;
}
