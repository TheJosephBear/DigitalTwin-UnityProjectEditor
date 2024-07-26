using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneList : MonoBehaviour {
    public static SceneList Instance { get; private set; }
    public List<SceneField> scenes = new List<SceneField>();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
        }
    }
    
    public SceneField GetScene(SceneType sceneType) {
        foreach (SceneField scene in scenes) {
            if (scene.SceneName == sceneType.ToString())
                return scene;
        }
        Debug.LogError("Sceme named " + sceneType + " aint in the databse chief...");
        return null;
    }
    
}
