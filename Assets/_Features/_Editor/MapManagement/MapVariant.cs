using System;
using UnityEngine;

public class MapVariant : MonoBehaviour {

    private string _name = "";
    private bool _isBaseMap = false;
    private bool _isLocked = false;
    private bool _isVisible = false;
    private ModelAsset _modelAsset;

    public void ToggleMeshVisibility(bool toggleOn) {
        gameObject.SetActive(true);
        foreach (Transform child in GetComponentsInChildren<Transform>()) {
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                child.GetComponent<MeshRenderer>().enabled = toggleOn;
            }
        }
        _isVisible = toggleOn;
    }

    public void SetMeshLayer(MapPriority priority) {
        switch (priority) {
            case MapPriority.Primary:
                AddLayerToAllChildren("PrimaryMap");
                break;
            case MapPriority.Secondary:
                AddLayerToAllChildren("SecondaryMap");
                break;
            default:
                print("Priority not implemented! (How did we get here?)");
                break;
        }
    }

    void AddLayerToAllChildren(string layerName) {
        foreach (Transform child in GetComponentsInChildren<Transform>()) {
            if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<MeshFilter>() != null) {
                if (child.GetComponent<MeshCollider>() == null) {
                    child.gameObject.layer = LayerMask.NameToLayer(layerName);
                }
            }
        }
    }

    public SerializableMapVariant Serialize() {
        return new SerializableMapVariant {
            modelID = ModelAsset.ModelID,
            isBaseMap = IsBaseMap,
        };
    }

    public void Deserialize(SerializableMapVariant serializedMap) {
        ModelAsset = AssetManager.Instance.FindModelAssetByID(serializedMap.modelID);
        IsBaseMap = serializedMap.isBaseMap;
    }


    #region GettersSetters

    // isLocked
    public bool IsLocked { get => _isLocked; set { _isLocked = value; print($"({Name}): Changing lockstate to "+value); } }

    // modelAsset
    public ModelAsset ModelAsset { get => _modelAsset; set => _modelAsset = value; }

    // isBaseMap
    public bool IsBaseMap { get => _isBaseMap; set => _isBaseMap = value; }
    public bool IsVisible { get => _isVisible; private set => _isVisible = value; }
    public string Name { get => _name; set => _name = value; }

    #endregion

}
public enum MapPriority {
    Primary,
    Secondary,
}

[Serializable]
public class SerializableMapVariant {
    public string modelID;
    public bool isBaseMap;
}