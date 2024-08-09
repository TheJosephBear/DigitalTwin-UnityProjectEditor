using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;
using System.IO;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ObjectUploadingManager : Singleton<ObjectUploadingManager> {

    protected override void Awake() {
        base.Awake();
    }

    public Vector3 mapLoadingPosition;
    public Vector3 decorationLoadingPosition;
    public Transform DecorationScrollViewObject;
    public Button UIDecorationButton;

    List<Decoration> Decorations = new List<Decoration>();
    public Decoration ActiveDecoration { get; private set; }
    GameObject Map;


    public void UploadMap(GameObject map) {
        Map = map;
    }

    public void CreateNewDecoration() {
        // Creates new button with the instance refference
        GameObject uiDecorButton = Instantiate(UIDecorationButton.gameObject);
        uiDecorButton.transform.SetParent(DecorationScrollViewObject);
        uiDecorButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        Decoration decoration = new Decoration();
        decoration.SetName("new Decoration");
        uiDecorButton.GetComponent<DecorationButton>().Initialize(decoration);
        Decorations.Add(decoration);
    }

    public void UploadNewDecorationModel(GameObject decorationModel) {
        ActiveDecoration.AddVariant(decorationModel);
    }

    public void SpawnDecoration() {
        ActiveDecoration.Spawn(decorationLoadingPosition);
    }

    public void SpawnMap() {
        // Instantiate(Map, mapLoadingPosition, Quaternion.identity).SetActive(true);
        Map.SetActive(true);
    }

    public void SetActiveDecoration(Decoration decoration) {
        ActiveDecoration = decoration;
    }

}