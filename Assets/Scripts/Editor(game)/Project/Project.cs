using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Project : Singleton<Project> {

    /// <summary>
    /// Stores data needed for editor to worko
    /// </summary>
    
    public string ProjectName { get; private set; }

    protected override void Awake() {
        base.Awake();
    }

    public void OpenProject(ProjectWebRefference projectWebReff) {
        GetComponent<ProjectSaver>().project = this;
        SetProjectName(projectWebReff.projectName);
    }


    public void SetProjectName(string projectName) {
        ProjectName = projectName;
    }

    public string SerializeProject() {
        SerializableProject serializableProject = new SerializableProject {
            projectName = ProjectName,
       //     decorationPresets = DecorationManager.Instance.SerializeDecorationPresets(),
       //     decorationsInstantiated = DecorationManager.Instance.SerializeDecorationsInstantiated(),
            map = MapManager.Instance.SerializeMap(),
            modelAssets = AssetManager.Instance.SerializeAssetList()
        };
        return JsonUtility.ToJson(serializableProject);
    }

    public Task<bool> DeserializeProjectAsync(string json) {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(DeserializeCoroutine(json, tcs)); // Pass the TaskCompletionSource to the coroutine
        return tcs.Task; // Return the task so it can be awaited
    }

    IEnumerator DeserializeCoroutine(string json, TaskCompletionSource<bool> tcs) {
        SerializableProject serializedProject = JsonUtility.FromJson<SerializableProject>(json);

        SetProjectName(serializedProject.projectName);

        // Wait for models to load
        bool isDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.modelAssets, () => {
            isDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isDeserializationComplete);

        // Optionally deserialize decorations and map
        // DecorationManager.Instance.DeserializeDecorationPresets(serializedProject.decorationPresets);
        // DecorationManager.Instance.DeserializeDecorationsInstantiated(serializedProject.decorationsInstantiated);
        MapManager.Instance.DeserializeMap(serializedProject.map);

        tcs.SetResult(true); // Complete the task when everything is done
    }



}

[Serializable]
public class SerializableProject {
    public string projectName;
 //   public List<SerializableDecorationPreset> decorationPresets;
  //  public List<SerializableDecoration> decorationsInstantiated;
    public SerializableMap map;
    public List<SerializableModelAsset> modelAssets;
}