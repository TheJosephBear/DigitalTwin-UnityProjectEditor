using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
     //   addedMap.AddComponent<Movable>();
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

    public void ToggleMapVisibility() {
        _baseMap.ToggleMeshVisibility(!_baseMap.IsVisible);
    }

    public void ClearEverything() {
        _baseMap = null;
        _mapVariants.Clear();
    }

    public bool hasVariant() {
        return _mapVariants.Count > 0;
    }

    public bool IsBaseMapUploaded() {
        return _baseMap != null;
    }

    public MapVariant GetBaseMap() {
        return _baseMap;
    }

    public List<MapVariant> GetVariants() {
        var allVariants = new List<MapVariant>(_mapVariants);
        if (_baseMap != null) allVariants.Add(_baseMap);
        return allVariants;
    }

    #region Serialization

    public SerializableMap Serialize() {
        if (_baseMap == null) return null;

        List<SerializableMapVariant> variantListSerialized = new List<SerializableMapVariant>();
        foreach (var variant in _mapVariants) {
            if (variant != null && variant.ModelAsset != null) {
                variantListSerialized.Add(variant.Serialize());
            }
        }

        return new SerializableMap {
       //     baseModelID = _baseMap.ModelAsset.ModelID,
            variants = variantListSerialized
        };
    }


    public void Deserialize(SerializableMap serializedMap) {
        if (serializedMap == null) return;

    //    var baseAsset = AssetManager.Instance.FindModelAssetByID(serializedMap.baseModelID);
        UploadBaseMapModel(AssetManager.Instance.FindModelAssetByID(serializedMap.baseMap.modelID));
        SpawnMap();

        foreach (var variant in serializedMap.variants) {
            var asset = AssetManager.Instance.FindModelAssetByID(variant.modelID);
            UploadMapVariant(asset);
        }
    }

    #endregion

}


[Serializable]
public class SerializableMap {
    public SerializableMapVariant baseMap;
    public List<SerializableMapVariant> variants;
}
