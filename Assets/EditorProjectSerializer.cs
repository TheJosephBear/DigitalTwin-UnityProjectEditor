using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorProjectSerializer : MonoBehaviour {

    public SerializableProject SerializeProject() {
        Project OpenedProject = ProjectManager.Instance.SelectedProject;

        SerializableProject serializableProject = new SerializableProject {
            projectId = OpenedProject.ProjectID,
            projectName = OpenedProject.ProjectName,
            serializedModelAssets = AssetManager.Instance.SerializeAssetList(),
            serializedMap = EditorManager.Instance.MapManager.Serialize(),
            serializedViewPointManager = EditorManager.Instance.ViewManager.Serialize()
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
        UImanager.Instance.ShowUI(UIType.LoadingScreen);

        SerializableProject serializedProject = project.SerializedProject;
        
        // Wait for asset manager
        bool isAssetDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.serializedModelAssets, () => {
            isAssetDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isAssetDeserializationComplete);

        // Deserialize everything else
        EditorManager.Instance.MapManager.Deserialize(serializedProject.serializedMap);
        EditorManager.Instance.ViewManager.Deserialize(serializedProject.serializedViewPointManager);

        UImanager.Instance.HideUI(UIType.LoadingScreen);
    }

}
