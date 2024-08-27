using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Project : Singleton<Project> {

    /// <summary>
    /// Stores all the project objects, their positions and metadata
    /// </summary>
    /// 
    public string ProjectName { get; private set; }

    protected override void Awake() {
        base.Awake();
        GetComponent<ProjectSaver>().project = this;
        SetProjectName("Awesome project");
    }

    public void SetProjectName(string projectName) {
        ProjectName = projectName;
    }

    public string SerializeProject() {
        SerializableProject serializableProject = new SerializableProject {
            projectName = ProjectName,
            decorationPresets = DecorationManager.Instance.SerializeDecorationPresets(),
            decorationsInstantiated = DecorationManager.Instance.SerializeDecorationsInstantiated(),
            map = MapManager.Instance.SerializeMap(),
            modelAssets = AssetManager.Instance.SerializeAssetList()
        };
        return JsonUtility.ToJson(serializableProject);
    }

    public void DeserializeProject(string json) {
        StartCoroutine(DeserializeCoroutine(json));
    }

    IEnumerator DeserializeCoroutine(string json) {
        SerializableProject serializedProject = JsonUtility.FromJson<SerializableProject>(json);

        SetProjectName(serializedProject.projectName);
        // Wait for models to load
        bool isDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.modelAssets, () => {
            isDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isDeserializationComplete);

        DecorationManager.Instance.DeserializeDecorationPresets(serializedProject.decorationPresets);
        DecorationManager.Instance.DeserializeDecorationsInstantiated(serializedProject.decorationsInstantiated);
        MapManager.Instance.DeserializeMap(serializedProject.map);
    }



}

[Serializable]
public class SerializableProject {
    public string projectName;
    public List<SerializableDecorationPreset> decorationPresets;
    public List<SerializableDecoration> decorationsInstantiated;
    public SerializableMap map;
    public List<SerializableModelAsset> modelAssets;
}