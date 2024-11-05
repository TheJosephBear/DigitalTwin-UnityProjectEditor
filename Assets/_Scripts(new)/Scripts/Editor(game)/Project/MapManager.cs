using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;

public class MapManager : Singleton<MapManager> {

    ModelAsset baseMap;
    List<ModelAsset> mapVariants = new List<ModelAsset>();
    public Vector3 mapSpawnPosition;

    GameObject currentMapVarInstance;

    //  List<GameObject> spinniiiieeee = new List<GameObject>();

    void Update() {
        /*
        foreach (GameObject go in spinniiiieeee) {
            //Spin it 
            go.transform.Rotate(0, 0 * Time.deltaTime, 100 * Time.deltaTime);
        }
        */
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
        /*
        if (go != null) spinniiiieeee.Add(go);
        if (go != null) go.transform.Rotate(new Vector3(-90, 0, 0));
        */
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


    public SerializableMap SerializeMap() {
        if (baseMap == null) {
            return null;
        }

        SerializableMap serializedMap = new SerializableMap {
            modelAssetID = baseMap.ModelID,
            spawnPosition = mapSpawnPosition
        };

        return serializedMap;
    }
    public void DeserializeMap(SerializableMap serializedMap) {
        if (serializedMap == null) return;

        ModelAsset mapAsset = AssetManager.Instance.FindModelAssetByID(serializedMap.modelAssetID);
        UploadBaseMapModel(mapAsset);
        mapSpawnPosition = serializedMap.spawnPosition;
        SpawnMap();
    }

}
[Serializable]
public class SerializableMap {
    public string modelAssetID;
    public Vector3 spawnPosition;
}
