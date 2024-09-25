using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using static UnityEditor.UIElements.ToolbarMenu;

public class DecorationUI : MonoBehaviour {

    public GameObject DecorationListButtonPrefab; // scrollview button prefab
    public GameObject DecorationListScrollview; // Scrollview showing decoration list in UI (content of scrollview)
    List<DecorationButton> DecorationListButtons = new List<DecorationButton>();
    public GameObject VariantUI;
    public GameObject VariantListButtonPrefab;
    public GameObject VariantListScrollview;
    List<VariantButton> VariantListButtons = new List<VariantButton>();
    public GameObject InstantiatedListButtonPrefab;
    public GameObject InstantiatedScrollview;
    List<DecorationInstantiatedButton> InstantiatedListButtons = new List<DecorationInstantiatedButton>();

    /* 
     * Decoration Logic 
     */

    public void onAddNewDecoration() {
        AudioManager.Instance.PlaySound(SoundType.click);
        DecorationPreset newDeco = DecorationManager.Instance.CreateNewDecorationPreset();
        DecorationButton newButton = AddButtonToDecorationList(newDeco);
        newButton.GetComponent<DecorationButton>().GetSelected();
        RefreshDecorationButtonList();
        UploadVariantFile();
    }

    public void UploadVariantFile() {
        FileBrowser.ShowLoadDialog(OnFileSelected, null, FileBrowser.PickMode.Files, false, null, "Select OBJ File", "Select");
    }

    void OnFileSelected(string[] paths) {
        if (paths.Length > 0) {
            string path = paths[0];
            if (Path.GetExtension(path).ToLower() == ".obj") {
                ModelAsset newModel = AssetManager.Instance.CreateNewAsset(path);
                DecorationManager.Instance.UploadNewDecorationVariant("New Variant", newModel);
                ToggleVariantUI(true);
            } else {
                PopUp.Instance.ShowPopUpWindow("Please select .obj file!");
            }
        }
    }

    public void RefreshDecorationButtonListSelection() {
        foreach (DecorationButton butt in DecorationListButtons) {
            DecorationPreset decoration = butt.decorationPreset;
            if (DecorationManager.Instance.GetActiveDecorationPreset() == decoration) {
                butt.GetSelected();
            } else {
                butt.GetUnselected();
            }
        }
    }

    public void RefreshDecorationButtonList() {
        foreach (DecorationButton butt in DecorationListButtons) {
            Destroy(butt.gameObject);
        }
        DecorationListButtons.Clear();
        foreach (DecorationPreset decoration in DecorationManager.Instance.GetDecorationsList()) {
            DecorationButton button = AddButtonToDecorationList(decoration);

            if (DecorationManager.Instance.GetActiveDecorationPreset() == decoration) {
                button.GetSelected();
            } else {
                button.GetUnselected();
            }
        }
    }

    DecorationButton AddButtonToDecorationList(DecorationPreset decoration) {
        GameObject uiDecorButton = Instantiate(DecorationListButtonPrefab.gameObject);
        uiDecorButton.transform.SetParent(DecorationListScrollview.transform);
        uiDecorButton.transform.localScale = new Vector3(1f, 1f, 1f);
        DecorationButton decoButtScript = uiDecorButton.GetComponent<DecorationButton>();
        decoButtScript.Initialize(decoration);
        DecorationListButtons.Add(decoButtScript);
        return decoButtScript;
    }

    /* 
     * Variant Logic 
     */

    public void ToggleVariantUI(bool show) {
        VariantUI.SetActive(show);
        if(show)
            RefreshButtonsToVariantList();
    }

    void RefreshButtonsToVariantList() {
        foreach (VariantButton butt in VariantListButtons) {
            Destroy(butt.gameObject);
        }
        VariantListButtons.Clear();
        foreach (DecorationVariant vari in DecorationManager.Instance.ActiveDecorationPreset.Variants) {
            GameObject button = Instantiate(VariantListButtonPrefab.gameObject);
            button.transform.SetParent(VariantListScrollview.transform);
            button.transform.localScale = new Vector3(1f, 1f, 1f);
            VariantButton VariButtScript = button.GetComponent<VariantButton>();
            VariButtScript.Initialize(vari);
            VariantListButtons.Add(VariButtScript);
        }
    }

    /* 
     * Instantiated logic 
     */

    public void RefreshInstantiatedList() {
        foreach (DecorationInstantiatedButton butt in InstantiatedListButtons) {
            Destroy(butt.gameObject);
        }
        InstantiatedListButtons.Clear();

        foreach (DecorationInstantiated decoration in DecorationManager.Instance.GetInstantiatedDecorationList()) {
            GameObject button = Instantiate(InstantiatedListButtonPrefab.gameObject);
            button.transform.SetParent(InstantiatedScrollview.transform);
            button.transform.localScale = new Vector3(1f, 1f, 1f);
            DecorationInstantiatedButton buttScript = button.GetComponent<DecorationInstantiatedButton>();
            buttScript.Initialize(decoration);
            InstantiatedListButtons.Add(buttScript);
        }
    }

}
