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
        GetComponent<ProjectSaver>().currentProject = this;
        SetProjectName("Krutopøísný projekt");
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
        print(serializableProject.modelAssets);
        return JsonUtility.ToJson(serializableProject);
    }

    public void DeserializeProject(string json) {
        SerializableProject serializedProject = JsonUtility.FromJson<SerializableProject>(json);

        // Set project name
        SetProjectName(serializedProject.projectName);

        // Deserialize ModelAssets
        AssetManager.Instance.DeserializeAssetList(serializedProject.modelAssets);

        // Deserialize DecorationPresets
        DecorationManager.Instance.DeserializeDecorationPresets(serializedProject.decorationPresets);

        // Deserialize DecorationsInstantiated
        DecorationManager.Instance.DeserializeDecorationsInstantiated(serializedProject.decorationsInstantiated);

        // Deserialize Map
        MapManager.Instance.DeserializeMap(serializedProject.map);
    }



}

[Serializable]
public class SerializableProject {
    public string projectName;
    public List<SerializableDecorationPreset> decorationPresets;
    public List<SerializableDecoration> decorationsInstantiated;
    public SerializableMap map;
    public string modelAssets; 
}