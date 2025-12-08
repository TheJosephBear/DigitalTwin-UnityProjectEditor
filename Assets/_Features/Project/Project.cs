using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Project : MonoBehaviour {
    public string ProjectID { get; private set; }
    public string ProjectName { get; private set; }
    public SerializableProject SerializedProject { get; private set; }

    // Deserialization for loading purposes
    public void CreateSerializedProjectFromJson(string json) {
        SerializedProject = JsonUtility.FromJson<SerializableProject>(json);
        ProjectID = SerializedProject.ProjectID;
        ProjectName = SerializedProject.ProjectName;
    }
}

[Serializable]
public class SerializableProject {
    public string ProjectID;
    public string ProjectName;
    public SerializableMap SerializedMap;
    public List<SerializableModelAsset> SerializedModelAssets;
    public SerializableViewPointManager SerializedViewPointManager;
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