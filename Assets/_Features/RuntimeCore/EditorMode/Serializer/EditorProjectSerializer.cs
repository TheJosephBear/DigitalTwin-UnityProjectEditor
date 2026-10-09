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
            serializableGeoMapManager = GeoMapManager.Instance.SerializeManager(),
            serializableSun = SunManager.Instance.Serialize()
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
        
        // Wait for asset manager
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializableModelAssets, () => {
            isAssetDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isAssetDeserializationComplete);

        // Deserialize everything else
        ImageManager.Instance.Deserialize(serializedProject.serializableTextureAssets);
        MapManager.Instance.Deserialize(serializedProject.serializableMapManager);
        ViewManager.Instance.Deserialize(serializedProject.serializableViewPointManager);
        GeoMapManager.Instance.DeserializeManager(serializedProject.serializableGeoMapManager);
        SunManager.Instance.Deserialize(serializedProject.serializableSun, GeoMapManager.Instance.GetCoordinates());

        UIManager.Instance.HideUI(UIType.LoadingScreen);
    }

}
