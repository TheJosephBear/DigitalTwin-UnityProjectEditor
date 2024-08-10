using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;
using System.IO;
using UnityEngine.UI;
using Unity.VisualScripting;
using static UnityEditor.PlayerSettings.WSA;

public class ObjectUploadingManager : Singleton<ObjectUploadingManager> {
    /// <summary>
    /// Uploads and spawns decorations and map
    /// </summary>
    public Vector3 mapLoadingPosition;
    public Vector3 decorationLoadingPosition;
    public Button spawnedDecorationButtonPrefab;
    public GameObject DecorationScrollViewObject;
    public Decoration ActiveDecorationPreset { get; private set; } // Selected decoration preset via ui

    protected override void Awake() {
        base.Awake();
    }

    public void UploadMap(GameObject map) {
        Project.Instance.AddMap(map);
    }

    public void SpawnMap() {
        Project.Instance.Map.SetActive(true);
    }

    public void CreateNewDecorationPreset() {
        Decoration decoration = new Decoration();
        decoration.SetName("new Decoration");
        Project.Instance.AddDecorationPreset(decoration);
        FindAnyObjectByType<EditorHUDui>().AddDecorationPrefabButton(decoration);
    }

    public void UploadNewDecorationModel(GameObject decorationModel) {
        ActiveDecorationPreset.AddVariant(decorationModel);
    }

    public void SpawnActiveDecoration() {
        GameObject deco = ActiveDecorationPreset.Spawn(decorationLoadingPosition);
        Project.Instance.AddDecorationInScene(deco);
        FindAnyObjectByType<EditorHUDui>().AddDecorationInSceneButton(deco);
    }


    public void SetActiveDecorationPreset(Decoration decoration) {
        ActiveDecorationPreset = decoration;
    }
    
}