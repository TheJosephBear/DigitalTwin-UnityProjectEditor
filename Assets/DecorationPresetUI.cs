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
        NameInputText.SetTextWithoutNotify(name);
    }

    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
        SaveSettings();
        // Reset the decoration UI to refresh the list
      //  UImanager.Instance.HideUI(UIType.DecorationMain);
      //  UImanager.Instance.ShowUI(UIType.DecorationMain);
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
        string originalName = ogName;
        string newName = originalName;
        int copyNumber = 1;

        // Check if the name exists, if not return it as is
        if (!DecorationManager.Instance.DecorationPresetNameExists(newName)) {
            print("the name is OG");
            return newName;
        }

        // Loop until a unique name is found
        while (DecorationManager.Instance.DecorationPresetNameExists(newName)) {
            print("Trying new name "+ newName);
            // Check if the name already has a copy number suffix in the format "(1)"
            int lastIndexOfOpenParenthesis = newName.LastIndexOf('(');
            int lastIndexOfCloseParenthesis = newName.LastIndexOf(')');

            // Check if the suffix is a valid number in parentheses at the end of the string
            if (lastIndexOfOpenParenthesis != -1 && lastIndexOfCloseParenthesis == newName.Length - 1) {
                string suffix = newName.Substring(lastIndexOfOpenParenthesis + 1, lastIndexOfCloseParenthesis - lastIndexOfOpenParenthesis - 1);
                if (int.TryParse(suffix, out int existingNumber)) {
                    // If a valid number is found, increment it
                    copyNumber = existingNumber + 1;
                    newName = newName.Substring(0, lastIndexOfOpenParenthesis).Trim();
                }
            }

            // Construct the new name with the incremented number
            newName = $"{newName} ({copyNumber})"; 
            DecorationManager.Instance.GetActiveDecorationPreset().SetName(UniqueNameEnsure(newName));
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
