using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewingSerializer : MonoBehaviour {

    // Tato metoda spustí proces a vrátí IEnumerator, aby ji Initializer mohl "yieldnout"
    public IEnumerator DeserializeProjectCoroutine(Project project) {
        SerializableProject serializedProject = project.SerializableProject;

        // One loading screen for the whole loading, its text says which part is loading
        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializableModelAssets, () => {
            isAssetDeserializationComplete = true;
        });

        yield return new WaitUntil(() => isAssetDeserializationComplete);

        LoadingScreenUI.SetStatus("Načítám obrázky…");
        bool isImageDeserializationComplete = false;
        ImageManager.Instance.Deserialize(serializedProject.serializableTextureAssets, () => {
            isImageDeserializationComplete = true;
        });

        yield return new WaitUntil(() => isImageDeserializationComplete);

        if (MapManager.Instance != null)
            yield return MapManager.Instance.DeserializeCoroutine(serializedProject.serializableMapManager);

        yield return EditorProjectSerializer.ShowStep("Načítám pohledy");
        if (ViewManager.Instance != null)
            ViewManager.Instance.Deserialize(serializedProject.serializableViewPointManager);

        if (GeoMapManager.Instance != null)
            GeoMapManager.Instance.DeserializeManager(serializedProject.serializableGeoMapManager);

        if (PinManager.Instance != null) {
            yield return EditorProjectSerializer.ShowStep("Načítám špendlíky");
            PinManager.Instance.Deserialize(serializedProject.serializablePinManager);
        }

        yield return EditorProjectSerializer.ShowStep("Načítám nastavení projektu");
        // Must run before FreecamState opens the sun UI, otherwise it gets created with the inspector defaults.
        // Coordinates come straight from the saved data: the geo managers are inactive in the Viewing scene.
        if (SunManager.Instance != null) {
            GeoLocalizationData geoData = serializedProject.serializableGeoMapManager?.geoData;
            Vector2 geoCoordinates = geoData != null ? new Vector2(geoData.latitude, geoData.longtitude) : Vector2.zero;
            SunManager.Instance.Deserialize(serializedProject.serializableSun, geoCoordinates);
        }

        if (serializedProject.serializableSettings != null)
            ProjectSettingsManager.Instance.Deserialize(serializedProject.serializableSettings);

        // Must finish before ViewingInitializer checks SurveyAllowed
        LoadingScreenUI.SetStatus("Kontroluji dotazník…");
        yield return ProjectSettingsManager.Instance.DetectExistingSurveyCoroutine();

        if (serializedProject.serializableFPS != null)
            FPSManager.Instance.Deserialize(serializedProject.serializableFPS);

        UIManager.Instance.HideUI(UIType.LoadingScreen);
    }
}
