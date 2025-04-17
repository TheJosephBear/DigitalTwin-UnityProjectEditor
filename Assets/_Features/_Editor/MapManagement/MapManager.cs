using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : Singleton<MapManager> {

    MapVariant _baseMap;
    List<MapVariant> _mapVariants = new List<MapVariant>();
    public Vector3 mapSpawnPosition;

    protected override void Awake() {
        base.Awake();
    }

    public void UploadBaseMapModel(ModelAsset newMap) {
        MapVariant addedMap = newMap.InstantiateModel(mapSpawnPosition).AddComponent<MapVariant>();
        addedMap.ModelAsset = newMap;
        addedMap.gameObject.SetActive(true);
        addedMap.IsBaseMap = true;
        _baseMap = addedMap;
        SpawnMap();
    }

    public void UploadMapVariant(ModelAsset newMap) {
        MapVariant addedMap = newMap.InstantiateModel(mapSpawnPosition).AddComponent<MapVariant>();
        addedMap.ModelAsset = newMap;
        _mapVariants.Add(addedMap);
    }

    public void SpawnMap() {
        _baseMap?.ToggleMeshVisibility(true);
    }

    public void ClearEverything() {
        _baseMap = null;
        _mapVariants.Clear();
    }

    public bool hasVariant() {
        return _mapVariants.Count > 0;
    }

    public List<MapVariant> GetVariants() {
        var allVariants = new List<MapVariant>(_mapVariants);
        if (_baseMap != null) allVariants.Add(_baseMap);
        return allVariants;
    }

    public SerializableMap Serialize() {
        if (_baseMap == null) return null;

        List<SerializableMapVariant> variantListSerialized = new List<SerializableMapVariant>();
        foreach (var variant in _mapVariants) {
            variantListSerialized.Add(variant.Serialize());
        }

        return new SerializableMap {
            baseModelID = _baseMap.ModelAsset.ModelID,
            variants = variantListSerialized
        };
    }

    public void Deserialize(SerializableMap serializedMap) {
        if (serializedMap == null) return;

        var baseAsset = AssetManager.Instance.FindModelAssetByID(serializedMap.baseModelID);
        UploadBaseMapModel(baseAsset);
        SpawnMap();

        foreach (var variant in serializedMap.variants) {
            var asset = AssetManager.Instance.FindModelAssetByID(variant.modelID);
            UploadMapVariant(asset);
        }
    }
}


[Serializable]
public class SerializableMap {
    public string baseModelID; // base map
    public List<SerializableMapVariant> variants;
}
