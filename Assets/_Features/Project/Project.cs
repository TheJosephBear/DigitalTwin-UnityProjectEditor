using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Project {
    public string ProjectID { get; private set; }
    public string ProjectName { get; private set; }
    public string ProjectDescription { get; private set; }
    public string ProjectImageID { get; private set; }
    public string Owner { get; private set; }
    public SerializableProject SerializableProject { get; private set; }
    public ProjectMetadata ProjectMetadata;

    public Project() { }
    public Project(string projectName) {
        ProjectName = projectName;
    }

    public void CreateSerializableProjectFromJson(string json) {
        SerializableProject = JsonUtility.FromJson<SerializableProject>(json);
        ProjectID = SerializableProject.projectId;
        ProjectName = SerializableProject.projectName;
        ProjectDescription = SerializableProject.projectDescription;
        ProjectImageID = SerializableProject.projectImageID;
        Owner = SerializableProject.owner;
    }
}

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// SERIALIZABLES MUST HAVE LOWERCASE FIRST LETTER VARIABLES BECAUSE OF THE WAY JSON UTILITY HANDLES IT
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

[Serializable]
public class SerializableProject {
    public string projectId;
    public string projectName;
    public string projectDescription;
    public string projectImageID;
    public string owner;
    public SerializableMapManager serializableMapManager;
    public List<SerializableModelAsset> serializableModelAssets;
    public List<serializableTextureAsset> serializableTextureAssets;
    public SerializableViewPointManager serializableViewPointManager;
    public SerializableGeoMapManager serializableGeoMapManager;
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
