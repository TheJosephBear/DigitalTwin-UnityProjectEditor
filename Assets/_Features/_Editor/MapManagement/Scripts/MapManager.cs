using System;
using System.Collections.Generic;
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
        Transform originalTransform = null;
        if (_baseMap != null) {
            originalTransform = _baseMap.gameObject.transform;
            AssetManager.Instance.DestroyAsset(_baseMap.GetComponent<ModelAsset>());
            _baseMap = null;
        }
        MapVariant addedMap = newMap.InstantiateModel(mapSpawnPosition).AddComponent<MapVariant>();
        addedMap.ModelAsset = newMap;
        addedMap.gameObject.SetActive(true);
        addedMap.IsBaseMap = true;
        addedMap.Name = newMap.FileName;
        //   addedMap.AddComponent<Movable>();
        if (originalTransform != null) {
            addedMap.gameObject.transform.position = originalTransform.position;
            addedMap.gameObject.transform.rotation = originalTransform.rotation;
        }
        _baseMap = addedMap;
        SpawnMap();
    }

    public void SetMapName(MapVariant map, string name) {
        map.Name = name;
    }

    public void ApplyAndSaveMapOffset(MapVariant map, Vector3 positionOffset, Vector3 rotationOffset) {
        print(map.transform.position + " " + map.transform.rotation);
        map.PositionOffset = positionOffset;
        map.RotationOffset = rotationOffset;

        map.transform.position = _baseMap.transform.TransformPoint(map.PositionOffset);
        Quaternion localRotation = Quaternion.Euler(map.RotationOffset);
        map.transform.rotation = _baseMap.transform.rotation * localRotation;
        print(map.transform.position + " " + map.transform.rotation);
    }

    public void UploadMapVariant(ModelAsset newMap) {
        MapVariant addedMap = newMap.InstantiateModel(mapSpawnPosition).AddComponent<MapVariant>();
        addedMap.ModelAsset = newMap;
        addedMap.Name = newMap.FileName;
        _mapVariants.Add(addedMap);
    }

    public void UploadMapVariantAgain(MapVariant oldMap, ModelAsset newModel) {
        if (oldMap == null || newModel == null) {
            Debug.LogError("UploadMapVariantAgain failed: oldMap or newModel is null.");
            return;
        }

        // 1. Find the index of the old variant in your tracking list
        int index = _mapVariants.IndexOf(oldMap);
        if (index == -1) {
            Debug.LogError("The map variant you are trying to replace is not tracked in _mapVariants!");
            return;
        }

        // 2. Capture the exact transform metrics of the old map variant
        Vector3 oldPosition = oldMap.gameObject.transform.position;
        Quaternion oldRotation = oldMap.gameObject.transform.rotation;
        string oldCustomName = oldMap.Name;

        // 3. Clean up the old asset tracking and destroy its GameObject
        if (oldMap.ModelAsset != null) {
            AssetManager.Instance.DestroyAsset(oldMap.ModelAsset);
            Destroy(oldMap.gameObject);
        }

        // 4. Instantiate the replacement model at the exact same location coordinates
        GameObject newGo = newModel.InstantiateModel(oldPosition);
        newGo.gameObject.transform.rotation = oldRotation;

        // 5. Construct and attach the new MapVariant properties
        MapVariant replacementMap = newGo.AddComponent<MapVariant>();
        replacementMap.ModelAsset = newModel;
        replacementMap.IsBaseMap = false;

        // Retain its old name if it was customized, otherwise fall back to new file name
        replacementMap.Name = string.IsNullOrEmpty(oldCustomName) ? newModel.FileName : oldCustomName;

        // 6. Swap the old reference with our new instance at the identical list index position
        _mapVariants[index] = replacementMap;

        print($"Successfully swapped map variant model to: {newModel.FileName}");
    }

    public void RemoveMapVariant(MapVariant map) {
        _mapVariants.Remove(map);
        Destroy(map.gameObject);
    }

    public void EnterVariantAdjusting(MapVariant map) {
        ToggleMapUI(false);
        EditorManager.Instance.ChangeState(AppState.VariantAdjusting);
        MapVariantAdjustManager.Instance.EnterAdjusting(map);
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
