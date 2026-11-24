using System.Collections;
using UnityEngine;

public class MapVariant : MonoBehaviour {

    private string _name = "";
    private bool _isBaseMap = false;
    private bool _isLocked = false;
    private bool _isVisible = false;
    // Absolute world position and rotation (Euler angles)
    private Vector3 _position;
    private Vector3 _rotation;
    private ModelAsset _modelAsset;

    public void AddCollider() {
        CoroutineUtils.RunSync(AddColliderCoroutine());
    }

    /// <summary>
    /// <see cref="AddCollider"/> spread over frames (each MeshCollider cooks its mesh), showing its progress on the loading screen.
    /// </summary>
    public IEnumerator AddColliderCoroutine() {
        Transform[] children = GetComponentsInChildren<Transform>();
        FrameBudget budget = new FrameBudget();
        for (int i = 0; i < children.Length; i++) {
            Transform child = children[i];
            if (child != null && child.GetComponent<MeshCollider>() == null) {
                child.gameObject.AddComponent<MeshCollider>();
            }
            if (budget.ShouldYield()) {
                LoadingScreenUI.SetStatus($"Vytvářím kolize {i + 1}/{children.Length}");
                yield return null;
            }
        }
    }

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
            name = Name,
            modelFileHash = ModelAsset != null ? ModelAsset.FileHash : "",
            isBaseMap = IsBaseMap,
            Position = Position, // Saves world position
            Rotation = Rotation  // Saves world rotation (Euler)
        };
    }

    public void Deserialize(SerializableMapVariant serializedMap) {
        ModelAsset = AssetManager.Instance.FindModelAssetByFileHash(serializedMap.modelFileHash);
        IsBaseMap = serializedMap.isBaseMap; 

        Position = serializedMap.Position;
        Rotation = serializedMap.Rotation;
        transform.position = Position;
        transform.rotation = Quaternion.Euler(Rotation);
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
    public Vector3 Position { get => _position; set => _position = value; }
    public Vector3 Rotation { get => _rotation; set => _rotation = value; }

    #endregion

}
public enum MapPriority {
    Primary,
    Secondary,
}
