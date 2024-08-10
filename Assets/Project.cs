using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Project : Singleton<Project> {

    /// <summary>
    /// Stores all the project objects, their positions and metadata
    /// </summary>
    /// 
    public string ProjectName { get; private set; }
    List<Decoration> DecorationPresets = new List<Decoration>();
    List<GameObject> DecorationsInScene = new List<GameObject>();
    public GameObject Map { get; private set; }

    protected override void Awake() {
        base.Awake();
        GetComponent<ProjectSaver>().currentProject = this;
    }

    public void SetProjectName(string projectName) {
        ProjectName = projectName;
    }
    
    public void AddMap(GameObject map) {
        Map = map;
    }

    public void AddDecorationPreset(Decoration decoration) {
        DecorationPresets.Add(decoration);
    }

    public void AddDecorationInScene(GameObject decoration) {
        DecorationsInScene.Add(decoration);
    }

    public string SerializeProject() {
        // Create a list to hold serialized GameObjects
        List<string> serializedObjects = new List<string>();

        // Serialize the map
        if (Map != null) {
            serializedObjects.Add(GameObjectSerializer.Serialize(Map));
        }

        // Serialize all decorations in the scene
        foreach (GameObject d in DecorationsInScene) {
            serializedObjects.Add(GameObjectSerializer.Serialize(d));
        }

        // Combine all serialized objects into a single JSON array
        return JsonUtility.ToJson(new SerializationWrapper(serializedObjects));
    }

    public void DeserializeProject(string json) {
        // Deserialize the JSON array
        SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);

        // Clear the existing map and decorations
        if (Map != null) {
            Destroy(Map);
        }
        DecorationsInScene.ForEach(Decoration => Destroy(Decoration));
        DecorationsInScene.Clear();

        // Deserialize and instantiate each GameObject
        foreach (string serializedObject in wrapper.Objects) {
            GameObject obj = GameObjectSerializer.Deserialize(serializedObject);
            if (obj.name == Map?.name) {
                Map = obj;
            } else {
                DecorationsInScene.Add(obj);
            }
        }
    }


}


[System.Serializable]
public class SerializationWrapper {
    public List<string> Objects;

    public SerializationWrapper(List<string> objects) {
        Objects = objects;
    }
}