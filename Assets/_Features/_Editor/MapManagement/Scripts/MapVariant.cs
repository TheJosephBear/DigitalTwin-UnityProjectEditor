using System;
using UnityEngine;

public class MapVariant : MonoBehaviour {

    private string _name = "";
    private bool _isBaseMap = false;
    private bool _isLocked = false;
    private bool _isVisible = false;
    private Vector3 _positionOffset; // Offset to base map (only for variants)
    private Vector3 _rotationOffset; // Offset to base map (only for variants)
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
            modelFileHash = ModelAsset.FileHash,
            isBaseMap = IsBaseMap,
        };
    }

    public void Deserialize(SerializableMapVariant serializedMap) {
        ModelAsset = AssetManager.Instance.FindModelAssetByFileHash(serializedMap.modelFileHash);
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
    public Vector3 PositionOffset { get => _positionOffset; set => _positionOffset = value; }
    public Vector3 RotationOffset { get => _rotationOffset; set => _rotationOffset = value; }

    #endregion

}
public enum MapPriority {
    Primary,
    Secondary,
}

[Serializable]
public class SerializableMapVariant {
    public string name;
    public string modelFileHash;
    public bool isBaseMap;
    [SerializeField] private float posX, posY, posZ;
    [SerializeField] private float rotX, rotY, rotZ;

    // Property to get/set them as a Vector3 in your code
    public Vector3 Position {
        get => new Vector3(posX, posY, posZ);
        set {
            posX = value.x;
            posY = value.y;
            posZ = value.z;
        }
    }

    public Vector3 Rotation {
        get => new Vector3(rotX, rotY, rotZ);
        set {
            rotX = value.x;
            rotY = value.y;
            rotZ = value.z;
        }
    }
}