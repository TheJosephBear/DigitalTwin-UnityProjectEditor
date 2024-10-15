using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Project : MonoBehaviour {

    /// <summary>
    /// Stores data needed for editor to worko
    /// </summary>
    
    public string ProjectName { get; private set; }

    public void OpenProject(ProjectWebRefference projectWebReff) {
        SetProjectName(projectWebReff.projectName);
    }
    
    public void CloseProject() {
        AssetManager.Instance.ClearEverything();
        DecorationManager.Instance.ClearEverything();
        MapManager.Instance.ClearEverything();
    }


    public void SetProjectName(string projectName) {
        ProjectName = projectName;
    }

    public string SerializeProject() {
        SerializableProject serializableProject = new SerializableProject {
            projectName = ProjectName,
            map = MapManager.Instance.SerializeMap(),
            modelAssets = AssetManager.Instance.SerializeAssetList(),
            decorationPresets = DecorationManager.Instance.SerializeDecorationPresets(),
            decorationsInstantiated = DecorationManager.Instance.SerializeDecorationsInstantiated()
        };
        return JsonUtility.ToJson(serializableProject);
    }

    public void DeserializeProject(string json) {
        StartCoroutine(DeserializeCoroutine(json));
    }

    IEnumerator DeserializeCoroutine(string json) {
        SerializableProject serializedProject = JsonUtility.FromJson<SerializableProject>(json);
        SetProjectName(serializedProject.projectName);
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
    public SerializableMap map;
    public List<SerializableModelAsset> modelAssets;
    public List<SerializableDecorationPreset> decorationPresets;
    public List<SerializableDecorationInstantiated> decorationsInstantiated;
}

[Serializable]
public class SerializableDecorationPreset {
    public string presetName;
    public List<SerializableDecorationVariant> variants;
}

[Serializable]
public class SerializableDecorationVariant {
    public string variantName;
    public string modelID;
}

[Serializable]
public class SerializableDecorationInstantiated {
    public string instanceName;
    public string presetName;
    public string variantName;
    public Vector3 position;
}