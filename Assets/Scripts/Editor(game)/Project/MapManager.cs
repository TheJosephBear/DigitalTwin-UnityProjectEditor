using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager> {

    ModelAsset map;
    public Vector3 mapSpawnPosition;

    public void UploadMapModel(ModelAsset newMap) {
        map = newMap;
        SpawnMap();
    }

    public void SpawnMap() {
        GameObject go = map?.InstantiateModel(mapSpawnPosition);
        go?.SetActive(true);
    }

    public void ClearEverything() {
        map = null;
    }

    public SerializableMap SerializeMap() {
        if (map == null) {
            return null;
        }

        SerializableMap serializedMap = new SerializableMap {
            modelAssetID = map.ModelID,
            spawnPosition = mapSpawnPosition
        };

        return serializedMap;
    }
    public void DeserializeMap(SerializableMap serializedMap) {
        if (serializedMap == null) return;

        ModelAsset mapAsset = AssetManager.Instance.FindModelAssetByID(serializedMap.modelAssetID);
        UploadMapModel(mapAsset);
        mapSpawnPosition = serializedMap.spawnPosition;
        SpawnMap();
    }

}
[Serializable]
public class SerializableMap {
    public string modelAssetID;
    public Vector3 spawnPosition;
}
