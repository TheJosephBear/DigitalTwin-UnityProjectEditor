using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Project : MonoBehaviour {

    /// <summary>
    /// Stores editor data
    /// </summary>
    
    public string ProjectName { get; private set; }

    // Opening the project in editor
    public void OpenProject(ProjectWebRefference projectWebReff) {
        SetProjectName(projectWebReff.projectName);
    }
    
    // Leaving from editor
    public void CloseProject() {
        AssetManager.Instance.ClearEverything();
        DecorationManager.Instance.ClearEverything();
        MapManager.Instance.ClearEverything();
        ViewManager.Instance.ClearEverything();
    }

    // Serialization for saving purposes
    public string SerializeProject() {
        SerializableProject serializableProject = new SerializableProject {
            projectName = ProjectName,
            modelAssets = AssetManager.Instance.SerializeAssetList(),
            map = MapManager.Instance.Serialize(),
            interestPointManager = ViewManager.Instance.Serialize()
       //     decorationPresets = DecorationManager.Instance.SerializeDecorationPresets(),
       //     decorationsInstantiated = DecorationManager.Instance.SerializeDecorationsInstantiated()
        };
        return JsonUtility.ToJson(serializableProject);
    }

    // Deserialization for loading purposes
    public Task<bool> DeserializeProjectAsync(string json) {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(DeserializeCoroutine(json, tcs)); 
        return tcs.Task; 
    }


    void SetProjectName(string projectName) {
        ProjectName = projectName;
    }

    IEnumerator DeserializeCoroutine(string json, TaskCompletionSource<bool> tcs) {
        SerializableProject serializedProject = JsonUtility.FromJson<SerializableProject>(json);
        SetProjectName(serializedProject.projectName);
        bool isDeserializationComplete = false;
        AssetManager.Instance.DeserializeAssetList(serializedProject.modelAssets, () => {
            isDeserializationComplete = true;
        });
        yield return new WaitUntil(() => isDeserializationComplete);

        //   DecorationManager.Instance.DeserializeDecorationPresets(serializedProject.decorationPresets);
        //    DecorationManager.Instance.DeserializeDecorationsInstantiated(serializedProject.decorationsInstantiated);
        print(MapManager.Instance.name);

        MapManager.Instance.Deserialize(serializedProject.map);
        ViewManager.Instance.Deserialize(serializedProject.interestPointManager);

        tcs.SetResult(true); // Complete the task when everything is done
    }
}

[Serializable]
public class SerializableProject {
    public string projectName;
    public SerializableMap map;
    public List<SerializableModelAsset> modelAssets;
    public SerializableInterestPointManager interestPointManager;
    //    public List<SerializableDecorationPreset> decorationPresets;
    //    public List<SerializableDecorationInstantiated> decorationsInstantiated;
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