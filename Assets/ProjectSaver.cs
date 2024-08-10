using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectSaver : Singleton<ProjectSaver> {

    public Project currentProject;

    public void SaveProject() {
        string serializedProject = currentProject.SerializeProject();
        print(serializedProject);
    }

    public void LoadProject() {
        string downloadedData = "";
        // Download data from server
        currentProject.DeserializeProject(downloadedData); 
    }

    

}


[Serializable]
public class SerializableDecoration {
    public string name;
    public List<int> variantIndices;  // Indices of the variants in the scene
}

[Serializable]
public class SerializableDecorationInstance {
    public int presetIndex;
    public int variantIndex;
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class SerializableProject {
    public string projectName;
    public List<SerializableDecoration> decorationPresets = new List<SerializableDecoration>();
    public List<SerializableDecorationInstance> decorationsInScene = new List<SerializableDecorationInstance>();
}
