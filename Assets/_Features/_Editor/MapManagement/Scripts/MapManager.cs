using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : Singleton<MapManager> {

    public GameObject MapUIPrefab;
    public Vector3 mapSpawnPosition;
    MapVariant _baseMap;
    List<MapVariant> _mapVariants = new List<MapVariant>();
    GameObject _mapUIInstance;

    // Do budoucna nastavovat spawn position po posunu v geo mapě
    private void Update() {
        if(_baseMap!=null) mapSpawnPosition = _baseMap.transform.position;
    }

    public void ToggleMapUI(bool toggleOn) {
        if (_mapUIInstance == null && toggleOn == true) {
            _mapUIInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(MapUIPrefab);
            _mapUIInstance.GetComponent<MapUI>().Initialize();
        } else {
            _mapUIInstance.SetActive(toggleOn);
            if (toggleOn)  _mapUIInstance.GetComponent<MapUI>().Initialize();
        }
    }

    public void SetBaseMapModel(ModelAsset newMap) {
        print("Setting base map model");
        MapVariant addedMap = newMap.InstantiateModel(mapSpawnPosition).AddComponent<MapVariant>();
        addedMap.ModelAsset = newMap;
        addedMap.gameObject.SetActive(true);
        addedMap.IsBaseMap = true;
        addedMap.Name = newMap.FileName;
        //   addedMap.AddComponent<Movable>();
        _baseMap = addedMap;
        SpawnMap();
    }

    public void SetMapName(MapVariant map, string name) {
        map.Name = name;
    }

    public void SetMapOffset(MapVariant map, Vector3 positionOffset, Vector3 rotationOffset) {
        map.PositionOffset = positionOffset;
        map.RotationOffset = rotationOffset;
    }

    public void UploadMapVariant(ModelAsset newMap) {
        MapVariant addedMap = newMap.InstantiateModel(mapSpawnPosition).AddComponent<MapVariant>();
        addedMap.ModelAsset = newMap;
        addedMap.Name = newMap.FileName;
        _mapVariants.Add(addedMap);
    }

    public void UploadMapVariantAgain(MapVariant oldMap, ModelAsset newModel) {
       
    }

    public void RemoveMapVariant(MapVariant map) {
        _mapVariants.Remove(map);
        Destroy(map.gameObject);
    }

    public void EnterVariantAdjusting(MapVariant map) {
        print("Entering adjusting thingie.");
    }

    public void SpawnMap() {
        _baseMap?.ToggleMeshVisibility(true);
    }

    public void ToggleMapVisibility() {
        _baseMap?.ToggleMeshVisibility(!_baseMap.IsVisible);
    }

    public void ClearEverything() {
        _mapVariants.Clear();
        _baseMap = null;
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

    public List<MapVariant> GetVariantsWithoutBase() {
        return new List<MapVariant>(_mapVariants);
    }

    public void SetBaseMapPositionAndRotation(Vector3 position, Quaternion rotation) {
        _baseMap.gameObject.transform.position = position;
        _baseMap.gameObject.transform.rotation = rotation;
    }

    #region Serialization

    public SerializableMapManager Serialize() {
        if (_baseMap == null) return null;

        List<SerializableMapVariant> variantListSerialized = new List<SerializableMapVariant>();
        foreach (var variant in _mapVariants) {
            if (variant != null && variant.ModelAsset != null) {
                variantListSerialized.Add(variant.Serialize());
            }
        }

        return new SerializableMapManager {
            baseMap = _baseMap.Serialize(),
            variants = variantListSerialized
        };
    }


    public void Deserialize(SerializableMapManager serializedMap) {
        if (serializedMap == null || serializedMap.baseMap == null) return;

    //    var baseAsset = AssetManager.Instance.FindModelAssetByFileHash(serializableMapManager.baseModelID);
        SetBaseMapModel(AssetManager.Instance.FindModelAssetByFileHash(serializedMap.baseMap.modelFileHash));
        SpawnMap();

        foreach (var variant in serializedMap.variants) {
            var asset = AssetManager.Instance.FindModelAssetByFileHash(variant.modelFileHash);
            UploadMapVariant(asset);
        }
    }

    #endregion

}


[Serializable]
public class SerializableMapManager {
    public SerializableMapVariant baseMap;
    public List<SerializableMapVariant> variants;
}
