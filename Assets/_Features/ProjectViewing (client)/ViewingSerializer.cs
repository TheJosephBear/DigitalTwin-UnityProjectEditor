using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewingSerializer : MonoBehaviour {

    // Tato metoda spustí proces a vrátí IEnumerator, aby ji Initializer mohl "yieldnout"
    public IEnumerator DeserializeProjectCoroutine(Project project) {
        SerializableProject serializedProject = project.SerializableProject;

        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializableModelAssets, () => {
            isAssetDeserializationComplete = true;
        });

        yield return new WaitUntil(() => isAssetDeserializationComplete);

        bool isImageDeserializationComplete = false;
        ImageManager.Instance.Deserialize(serializedProject.serializableTextureAssets, () => {
            isImageDeserializationComplete = true;
        });

        yield return new WaitUntil(() => isImageDeserializationComplete);


        if (ViewingManager.Instance.MapManager != null)
            ViewingManager.Instance.MapManager.Deserialize(serializedProject.serializableMapManager);

        if (ViewingManager.Instance.ViewManager != null)
            ViewingManager.Instance.ViewManager.Deserialize(serializedProject.serializableViewPointManager);

        if (ViewingManager.Instance.GeoMapManager != null)
            ViewingManager.Instance.GeoMapManager.DeserializeManager(serializedProject.serializableGeoMapManager);
    }
}