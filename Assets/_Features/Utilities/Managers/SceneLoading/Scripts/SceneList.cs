using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneList : Singleton<SceneList> {

    public List<SceneField> scenes = new List<SceneField>();

    protected override void Awake() {
        base.Awake();
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
