using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;

public class MapUI : UIBehaviour {

    public GameObject BaseMapItemPrefab;
    public GameObject VariantMapItemPrefab;
    public GameObject BaseMapUploadButtonRef;
    public GameObject BaseMapItemParentRef;
    public GameObject VariantItemParentRef;

    ModelItem _baseMapItemInstance;
    List<ModelItem> _modelVariantItems = new List<ModelItem>();

    public void Initialize() {
        UpdateItems();
    }

    public void UpdateItems() {
        // Base map
        MapVariant baseMap = MapManager.Instance.GetBaseMap();
        if (baseMap != null) {
            BaseMapUploadButtonRef.SetActive(false);
            if (_baseMapItemInstance == null) {
                _baseMapItemInstance = Instantiate(BaseMapItemPrefab, BaseMapItemParentRef.transform).GetComponent<ModelItem>();
            } else {
                _baseMapItemInstance.gameObject.SetActive(true);
            }
            _baseMapItemInstance.Initialize(this, baseMap);
        } else {
            BaseMapUploadButtonRef.SetActive(true);
            if (_baseMapItemInstance != null) _baseMapItemInstance.gameObject.SetActive(false);
        }

        // Variants
        Utilities.DestroyAllGameObjects(_modelVariantItems);
        _modelVariantItems.Clear();
        foreach(MapVariant variant in MapManager.Instance.GetVariants()) {
            ModelItem variantModelItem = Instantiate(VariantMapItemPrefab, VariantItemParentRef.transform).GetComponent<ModelItem>();
            _modelVariantItems.Add(variantModelItem);
            variantModelItem.Initialize(this, variant);
        }
    }

    #region Button OnClicks

    public void OnX() {
        MapManager.Instance.ToggleMapUI(false);
    }

    #region BaseMap

    public void OnUploadBaseMap() {
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            EditorManager.Instance.MapManager.SetBaseMapModel(createdAsset);
            UpdateItems();
        });
    }

    public void OnRenameBaseMap(MapVariant mapVariant, string text) {
        MapManager.Instance.SetMapName(mapVariant, text);
        UpdateItems();
    }

    #endregion

    #region Variants

    public void OnAddVariant() {
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            EditorManager.Instance.MapManager.UploadMapVariant(createdAsset);
            UpdateItems();
        });
    }

    public void OnRenameVariant(MapVariant mapVariant, string text) {
        MapManager.Instance.SetMapName(mapVariant, text);
        UpdateItems();
    }

    public void OnUploadVariantAgain(MapVariant mapVariant) {
        ModelUploadManager.Instance.AskForModel((createdAsset) => {
            MapManager.Instance.UploadMapVariantAgain(mapVariant, createdAsset);
            UpdateItems();
        });
    }

    public void OnAdjustVariantGeoPosition(MapVariant mapVariant) {
        MapManager.Instance.EnterVariantAdjusting(mapVariant);
    }

    public void OnRemoveVariant(MapVariant mapVariant) {
        MapManager.Instance.RemoveMapVariant(mapVariant);
        UpdateItems();
    }

    #endregion

    #endregion

}