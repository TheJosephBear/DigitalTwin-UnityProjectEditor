using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorProjectSerializer : MonoBehaviour {

    public SerializableProject SerializeProject() {
   //     MessageDisplayManager.Instance.DisplayMessage("Serializace projektu");
        Project OpenedProject = ProjectManager.Instance.SelectedProject;

        SerializableProject serializableProject = new SerializableProject {
            projectId = OpenedProject.ProjectID,
            projectName = OpenedProject.ProjectName,
            projectDescription = OpenedProject.ProjectDescription,
            projectImageID = OpenedProject.ProjectImageID,
            owner = OpenedProject.Owner,
            serializableModelAssets = AssetManager.Instance.SerializeAssetList(),
            serializableTextureAssets = ImageManager.Instance.SerializeTextureList(),
            serializableMapManager = MapManager.Instance.Serialize(),
            serializableViewPointManager = ViewManager.Instance.Serialize(),
            serializablePinManager = PinManager.Instance != null ? PinManager.Instance.Serialize() : null,
            serializableGeoMapManager = GeoMapManager.Instance.SerializeManager(),
            serializableSun = SunManager.Instance.Serialize(),
            serializableSettings = ProjectSettingsManager.Instance.Serialize(),
            serializableFPS = FPSManager.Instance.Serialize()
        };
        return serializableProject;
    }

    public void DeserializeProject(Project project) {
        StartCoroutine(DeserializeCoroutine(project));
    }

    public IEnumerator DeserializeProjectCoroutinable(Project project) {
        yield return StartCoroutine(DeserializeCoroutine(project));
    }

    IEnumerator DeserializeCoroutine(Project project) {
    //    MessageDisplayManager.Instance.DisplayMessage("Deserializace projektu");
        UIManager.Instance.ShowUI(UIType.LoadingScreen);

        SerializableProject serializedProject = project.SerializableProject;
        
        // Wait for asset _manager
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializableModelAssets, () => {
            isAssetDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isAssetDeserializationComplete);

        // Deserialize everything else
        // A failing part is logged and skipped so the rest still loads; the user is told at the end
        List<string> failedParts = new List<string>();
        TryDeserialize("obrázky", failedParts, () =>
            ImageManager.Instance.Deserialize(serializedProject.serializableTextureAssets));
        TryDeserialize("podkladová mapa a varianty", failedParts, () =>
            MapManager.Instance.Deserialize(serializedProject.serializableMapManager));
        TryDeserialize("pohledy", failedParts, () =>
            ViewManager.Instance.Deserialize(serializedProject.serializableViewPointManager));
        if (PinManager.Instance != null)
            TryDeserialize("špendlíky", failedParts, () =>
                PinManager.Instance.Deserialize(serializedProject.serializablePinManager));
        TryDeserialize("umístění na mapě", failedParts, () =>
            GeoMapManager.Instance.DeserializeManager(serializedProject.serializableGeoMapManager));
        TryDeserialize("nastavení slunce", failedParts, () =>
            SunManager.Instance.Deserialize(serializedProject.serializableSun, GeoMapManager.Instance.GetCoordinates()));
        if (serializedProject.serializableFPS != null)
            TryDeserialize("pohyb v první osobě", failedParts, () =>
                FPSManager.Instance.Deserialize(serializedProject.serializableFPS));
        if(serializedProject.serializableSettings != null)
            TryDeserialize("nastavení projektu", failedParts, () =>
                ProjectSettingsManager.Instance.Deserialize(serializedProject.serializableSettings));
        yield return ProjectSettingsManager.Instance.DetectExistingSurveyCoroutine();
        // this hurts my soul
        CameraManager.Instance.ToggleCameraCollision(ProjectSettingsManager.Instance.EditorCameraCollision);
        FindAnyObjectByType<EditorHUDui>().CameraCollisionToggleRef.SetWithoutNotify(ProjectSettingsManager.Instance.EditorCameraCollision);

        UIManager.Instance.HideUI(UIType.LoadingScreen);

        if (failedParts.Count > 0) {
            PopUp.Instance.ShowPopUpWindow(
                "Při načítání projektu došlo k chybě. Nepodařilo se načíst: " + string.Join(", ", failedParts) + ".\n" +
                "Pokud projekt uložíte, tyto části se mohou ztratit.");
        }
    }

    void TryDeserialize(string partName, List<string> failedParts, System.Action deserialize) {
        try {
            deserialize();
        } catch (System.Exception e) {
            Debug.LogException(e);
            failedParts.Add(partName);
        }
    }

}
