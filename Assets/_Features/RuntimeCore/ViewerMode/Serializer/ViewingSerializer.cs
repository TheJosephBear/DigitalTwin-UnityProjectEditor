using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewingSerializer : MonoBehaviour {

    // Tato metoda spustí proces a vrátí IEnumerator, aby ji Initializer mohl "yieldnout"
    public IEnumerator DeserializeProjectCoroutine(Project project) {
        SerializableProject serializedProject = project.SerializableProject;

        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializableModelAssets, () => {
            isAssetDeserializationComplete = true;
            UIManager.Instance.HideUI(UIType.LoadingScreen);
        });

        yield return new WaitUntil(() => isAssetDeserializationComplete);

        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        bool isImageDeserializationComplete = false;
        ImageManager.Instance.Deserialize(serializedProject.serializableTextureAssets, () => {
            isImageDeserializationComplete = true;
            UIManager.Instance.HideUI(UIType.LoadingScreen);
        });

        yield return new WaitUntil(() => isImageDeserializationComplete);


        if (MapManager.Instance != null)
            MapManager.Instance.Deserialize(serializedProject.serializableMapManager);

        if (ViewManager.Instance != null)
            ViewManager.Instance.Deserialize(serializedProject.serializableViewPointManager);

        if (GeoMapManager.Instance != null)
            GeoMapManager.Instance.DeserializeManager(serializedProject.serializableGeoMapManager);

        if (serializedProject.serializableSettings != null)
            ProjectSettingsManager.Instance.Deserialize(serializedProject.serializableSettings);
    }
}
