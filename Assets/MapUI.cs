using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;

public class MapUI : UIBehaviour {


    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.HideUI(UIType.Map);
    }

    public void onNahrat() {
        AudioManager.Instance.PlaySound(SoundType.click);
        FileBrowser.ShowLoadDialog(OnFileSelectedMap, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void onPridatVariantu() {
        FileBrowser.ShowLoadDialog(OnFileSelectedMapVar, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void onUkazVedleSebe() {
        MapManager.Instance.SpawnMap();
        MapManager.Instance.SpawnMapVariant();
        UImanager.Instance.ShowUI(UIType.TwoMapCamera);
    }

    void OnFileSelectedMap(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                MapManager.Instance.UploadBaseMapModel(AssetManager.Instance.CreateNewAsset(path));
            } else {
                PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
            }
        }
    }

    void OnFileSelectedMapVar(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                MapManager.Instance.UploadMapVariant(AssetManager.Instance.CreateNewAsset(path));
            } else {
                PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
            }
        }
    }

    public override void Show() {
        canvas.SetActive(true);
    }

    public override void Hide() {
        canvas.SetActive(false);
    }
}