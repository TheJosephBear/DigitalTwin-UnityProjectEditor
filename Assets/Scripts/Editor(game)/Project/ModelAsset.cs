using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelAsset : MonoBehaviour {
    /// <summary>
    /// All uploaded models have this script attached for easier save/load manipulation. 
    /// When serializing instantiated objects in scene they will refference only the ID of the model.
    /// </summary>
    
    public string ModelID { get; set; }
    public GameObject ModelGameObject { get; private set; }
    public string FileHash { get; set; }
    public string filePath;

    public void GenerateUniqueID() {
        ModelID = Guid.NewGuid().ToString();
    }

    public void SetModelGameObject(GameObject modelGameObject) {  ModelGameObject = modelGameObject; }

    public GameObject InstantiateModel(Vector3 pos) {
        return SceneLoadingManager.Instance.InstantiateObjectInScene(ModelGameObject, pos, SceneType.Editing);
    }

}

[Serializable]
public class SerializableModelAsset {
    public string modelID;
    public string fileHash;
}