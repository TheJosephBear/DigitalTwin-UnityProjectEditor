using System.Collections.Generic;
using UnityEngine;

public class MapDisplayManager : Singleton<MapDisplayManager> {

    private MapVariant _primaryMapInstance;
    private MapVariant _secondaryMapInstance;

    public void ShowVariant(MapVariant originalVariant, MapPriority priority) {
        if (originalVariant == null) {
            Debug.LogWarning("Cannot display null map variant.");
            return;
        }

        switch (priority) {
            case MapPriority.Primary:
                if (_primaryMapInstance != null)
                    Destroy(_primaryMapInstance.gameObject);
                break;

            case MapPriority.Secondary:
                if (_secondaryMapInstance != null)
                    Destroy(_secondaryMapInstance.gameObject);
                break;
        }

        GameObject cloneGO = Instantiate(originalVariant.gameObject, MapManager.Instance.mapSpawnPosition, Quaternion.identity);

        MapVariant clone = cloneGO.AddComponent<MapVariant>();
        clone.ModelAsset = originalVariant.ModelAsset;
        clone.ToggleMeshVisibility(true);
        clone.SetMeshLayer(priority);

        switch (priority) {
            case MapPriority.Primary:
                _primaryMapInstance = clone;
                break;

            case MapPriority.Secondary:
                _secondaryMapInstance = clone;
                break;
        }
    }

    public void LockMap(MapPriority priority, bool isLocked) {
        switch (priority) {
            case MapPriority.Primary:
                if (_primaryMapInstance != null)
                    _primaryMapInstance.IsLocked = isLocked;
                break;

            case MapPriority.Secondary:
                if (_secondaryMapInstance != null)
                    _secondaryMapInstance.IsLocked = isLocked;
                break;
        }
    }

    public void ClearDisplayedMaps() {
        if (_primaryMapInstance != null)
            Destroy(_primaryMapInstance.gameObject);
        if (_secondaryMapInstance != null)
            Destroy(_secondaryMapInstance.gameObject);

        _primaryMapInstance = null;
        _secondaryMapInstance = null;
    }
}
