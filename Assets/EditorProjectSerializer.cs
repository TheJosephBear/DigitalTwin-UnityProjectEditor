using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EditorProjectSerializer : MonoBehaviour {

    public SerializableProject SerializeProject() {
    //    MessageDisplayManager.Instance.DisplayMessage("Serializace projektu");
        Project OpenedProject = ProjectManager.Instance.SelectedProject;

        SerializableProject serializableProject = new SerializableProject {
            projectId = OpenedProject.ProjectID,
            projectName = OpenedProject.ProjectName,
            serializableModelAssets = AssetManager.Instance.SerializeAssetList(),
            serializableTextureAssets = ImageManager.Instance.SerializeTextureList(),
            serializableMapManager = EditorManager.Instance.MapManager.Serialize(),
            serializableViewPointManager = EditorManager.Instance.ViewManager.Serialize(),
            serializableGeoMapManager = EditorManager.Instance.GeoMapManager.SerializeManager()
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
        EditorManager.Instance.MapManager.Deserialize(serializedProject.serializableMapManager);
        EditorManager.Instance.ViewManager.Deserialize(serializedProject.serializableViewPointManager);
        EditorManager.Instance.GeoMapManager.DeserializeManager(serializedProject.serializableGeoMapManager);

        UIManager.Instance.HideUI(UIType.LoadingScreen);
    }

}
