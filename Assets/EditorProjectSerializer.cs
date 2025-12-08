using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorProjectSerializer : MonoBehaviour {

    public SerializableProject SerializeProject() {
        Project OpenedProject = ProjectManager.Instance.SelectedProject;

        SerializableProject serializableProject = new SerializableProject {
            ProjectID = OpenedProject.ProjectID,
            ProjectName = OpenedProject.ProjectName,
            SerializedModelAssets = AssetManager.Instance.SerializeAssetList(),
            SerializedMap = EditorManager.Instance.MapManager.Serialize(),
            SerializedViewPointManager = EditorManager.Instance.ViewManager.Serialize()
        };
        return serializableProject;
    }

    public void DeserializeProject(Project project) {
        StartCoroutine(DeserializeCoroutine(project));
    }

    IEnumerator DeserializeCoroutine(Project project) {
        UImanager.Instance.ShowUI(UIType.LoadingScreen);

        SerializableProject serializedProject = project.SerializedProject;
        
        // Wait for asset manager
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.SerializedModelAssets, () => {
            isAssetDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isAssetDeserializationComplete);

        // Deserialize everything else
        EditorManager.Instance.MapManager.Deserialize(serializedProject.SerializedMap);
        EditorManager.Instance.ViewManager.Deserialize(serializedProject.SerializedViewPointManager);

        UImanager.Instance.HideUI(UIType.LoadingScreen);
    }

}
