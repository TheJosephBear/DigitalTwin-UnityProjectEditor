using TMPro;
using UnityEngine;

public class ModelItem : MonoBehaviour {

    public TMP_InputField NameInput;
    public bool IsBaseMap = false;
    MapVariant _mapVariant;
    MapUI _UIScript;
    bool _isInitializing = false;

    public void Initialize(MapUI mapUIScript, MapVariant mapVariant = null) {
        _mapVariant = mapVariant;
        _UIScript = mapUIScript;

        _isInitializing = true;
        NameInput.text = _mapVariant.name;
        _isInitializing = false;
    }

    public void OnRename(string text) {
        if (_isInitializing) return;

        if (IsBaseMap) {
            _UIScript.OnRenameBaseMap(_mapVariant, text);
        } else {
            _UIScript.OnRenameVariant(_mapVariant, text);
        }
    }

    public void OnUpload() {
        if (IsBaseMap) {
            _UIScript.OnUploadBaseMap();
        } else {
            _UIScript.OnUploadVariantAgain(_mapVariant);
        }
    }

    public void OnAdjustPosition() {
        _UIScript.OnAdjustVariantGeoPosition(_mapVariant);
    }

    public void OnRemove() {
        _UIScript.OnRemoveVariant(_mapVariant);
    }
}
