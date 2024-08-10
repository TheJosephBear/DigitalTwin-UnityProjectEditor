using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;

public class DecorationUI : UIBehaviour {

    Decoration decoration;
    public GameObject canvas;

    public void SetDecoration(Decoration decoration) {
        this.decoration = decoration;
    }

    public void AddVariant() {
        FileBrowser.ShowLoadDialog(OnFileSelectedObject, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void SpawnVariantMain() {
        ObjectUploadingManager.Instance.SpawnActiveDecoration();
        HideUI();
    }

    public void HideUI() {
        UImanager.Instance.HideUI(UIType.DecorationPopUp);
    }

    void OnFileSelectedObject(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                GameObject loadingObject = FileLoading.Instance.LoadModel(path);
                ObjectUploadingManager.Instance.UploadNewDecorationModel(loadingObject);
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
