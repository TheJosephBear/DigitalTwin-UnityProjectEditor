using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;
using FrostweepGames.Plugins.WebGLFileBrowser;

public class MapUI : UIBehaviour {


    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.HideUI(UIType.Map);
    }

    public void onNahrat() {
        AudioManager.Instance.PlaySound(SoundType.click);
        //    FileBrowser.ShowLoadDialog(OnFileSelectedMap, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMap);
    }

    public void onPridatVariantu() {
        //    FileBrowser.ShowLoadDialog(OnFileSelectedMapVar, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
        FileBrowserManager.Instance.ShowLoadDialog(OnFileSelectedMapVar);
    }


    void OnFileSelectedMap(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            MapManager.Instance.UploadBaseMapModel(AssetManager.Instance.CreateNewAsset(files[0].fileInfo.path));
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }

    }

    void OnFileSelectedMapVar(FrostweepGames.Plugins.WebGLFileBrowser.File[] files) {
        if (files != null && files.Length > 0) {
            MapManager.Instance.UploadMapVariant(AssetManager.Instance.CreateNewAsset(files[0].fileInfo.path));
        } else {
            PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
        }
    }

    public override void Show() {
        canvas.SetActive(true);
    }

    public override void Hide() {
        canvas.SetActive(false);
    }
}