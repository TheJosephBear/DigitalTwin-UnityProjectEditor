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
        ImageManager.Instance.Deserialize(serializedProject.serializableTextureAssets);
        MapManager.Instance.Deserialize(serializedProject.serializableMapManager);
        ViewManager.Instance.Deserialize(serializedProject.serializableViewPointManager);
        if (PinManager.Instance != null)
            PinManager.Instance.Deserialize(serializedProject.serializablePinManager);
        GeoMapManager.Instance.DeserializeManager(serializedProject.serializableGeoMapManager);
        SunManager.Instance.Deserialize(serializedProject.serializableSun, GeoMapManager.Instance.GetCoordinates());
        if (serializedProject.serializableFPS != null)
            FPSManager.Instance.Deserialize(serializedProject.serializableFPS);
        if(serializedProject.serializableSettings != null)
            ProjectSettingsManager.Instance.Deserialize(serializedProject.serializableSettings);
        yield return ProjectSettingsManager.Instance.DetectExistingSurveyCoroutine();
        // this hurts my soul
        CameraManager.Instance.ToggleCameraCollision(ProjectSettingsManager.Instance.EditorCameraCollision);
        FindAnyObjectByType<EditorHUDui>().CameraCollisionToggleRef.SetWithoutNotify(ProjectSettingsManager.Instance.EditorCameraCollision);

        UIManager.Instance.HideUI(UIType.LoadingScreen);
    }

}
