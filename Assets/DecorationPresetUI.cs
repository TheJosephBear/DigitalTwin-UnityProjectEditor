using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;
using TMPro;
using JetBrains.Annotations;
using UnityEngine.UI;

public class DecorationPresetUI : UIBehaviour {

    public GameObject canvas;
    public TMP_InputField NameInputText;

    void Initialize() {
        string name = DecorationManager.Instance.GetActiveDecorationPreset().Name;
        NameInputText.SetTextWithoutNotify(UniqueNameEnsure(name));
    }

    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
        SaveSettings();
        // Reset the decoration UI to refresh the list
        UImanager.Instance.HideUI(UIType.DecorationMain);
        UImanager.Instance.ShowUI   (UIType.DecorationMain);
        // die :(
        UImanager.Instance.HideUI(UIType.DecorationPopUp);
    }

    public void onPridatVariantu() {
        FileBrowser.ShowLoadDialog(NewVariantAdded, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void onPridatDoSceny() {

    }

    public void onNahratModel() {
        FileBrowser.ShowLoadDialog(VariantEdited, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    public void onOdstranit() {

    }
        

    void SaveSettings() {
        // Save name
        SaveName();
    }

    void SaveName() {
        DecorationManager.Instance.GetActiveDecorationPreset().SetName(UniqueNameEnsure(NameInputText.text));
    }

    string UniqueNameEnsure(string ogName) {
        // Name must be original, if it isnt add "(1)" behind the copy name
        string originalName = ogName;
        string newName = originalName;
        int copyNumber = 1;
        if (!DecorationManager.Instance.DecorationPresetNameExists(newName)) {
            return newName;
        }
        while (DecorationManager.Instance.DecorationPresetNameExists(newName)) {
            int lastIndexOfOpenParenthesis = newName.LastIndexOf('(');
            int lastIndexOfCloseParenthesis = newName.LastIndexOf(')');
            if (lastIndexOfOpenParenthesis != -1 && lastIndexOfCloseParenthesis == newName.Length - 1) {
                string suffix = newName.Substring(lastIndexOfOpenParenthesis + 1, lastIndexOfCloseParenthesis - lastIndexOfOpenParenthesis - 1);
                if (int.TryParse(suffix, out int existingNumber)) {
                    copyNumber = existingNumber + 1;
                }
                newName = newName.Substring(0, lastIndexOfOpenParenthesis).Trim();
            }
            newName = $"{newName} ({copyNumber})";
            copyNumber++;
        }
        return newName;
    }

    public void SpawnVariant() {
        // Handle selecting the variant and spawning the chosen one

        // DecorationManager.Instance.SpawnActiveDecoration();
        // HideUI();
    }

    void HideUI() {
        UImanager.Instance.HideUI(UIType.DecorationPopUp);
    }

    void NewVariantAdded(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                ModelAsset modelAsset = AssetManager.Instance.CreateNewAsset(path);
                DecorationManager.Instance.UploadNewDecorationModel(modelAsset);
            }
        }
    }

    void VariantEdited(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                ModelAsset modelAsset = AssetManager.Instance.CreateNewAsset(path);
                DecorationManager.Instance.UploadNewDecorationModel(modelAsset);
            }
        }
    }

    public override void Show() {
        canvas.SetActive(true);
        Initialize();
    }

    public override void Hide() {
        canvas.SetActive(false);
    }
}
