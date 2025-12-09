using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Project : MonoBehaviour {
    public string ProjectID { get; private set; }
    public string ProjectName { get; private set; }
    public SerializableProject SerializedProject { get; private set; }

    public void CreateSerializedProjectFromJson(string json) {
        print("CREATE SERIALIZED PROJECT IN PROJECT CLASS");
        SerializedProject = JsonUtility.FromJson<SerializableProject>(json);
        print("serialized project:");
        print("ID: " + SerializedProject.projectId);
        print("Name: " + SerializedProject.projectName);
        ProjectID = SerializedProject.projectId;
        ProjectName = SerializedProject.projectName;
    }
}

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// SERIALIZABLES MUST HAVE LOWERCASE FIRST LETTER BECAUSE OF THE WAY JSON UTILITY HANDLES IT
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

[Serializable]
public class SerializableProject {
    public string projectId;
    public string projectName;
    public SerializableMap serializedMap;
    public List<SerializableModelAsset> serializedModelAssets;
    public SerializableViewPointManager serializedViewPointManager;
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