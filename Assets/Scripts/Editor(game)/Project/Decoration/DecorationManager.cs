using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DecorationManager : Singleton<DecorationManager> {
    
    List<DecorationPreset> DecorationPresets = new List<DecorationPreset>();
    List<DecorationInstantiated> DecorationsInstantiated = new List<DecorationInstantiated>();
    public DecorationPreset ActiveDecorationPreset { get; private set; } // what is Selected in the DecorationUI
    public Vector3 DecorationSpawnPos;

    public DecorationPreset CreateNewDecorationPreset() {
        DecorationPreset decorationPreset = new DecorationPreset();
        decorationPreset.SetName("NewDecoration");
        AddDecorationPreset(decorationPreset);
        return decorationPreset;
    }

    public void ToggleDecorationVariantEditorMenu(bool show) {
        if (show && ActiveDecorationPreset == null) 
            return;
        FindAnyObjectByType<DecorationUI>().ToggleVariantUI(show);
    }

    public void RenameSelectedDecoration(string name) {
        ActiveDecorationPreset.SetName(UniqueNameEnsure(name, DecorationEnsureType.Preset));
        FindAnyObjectByType<DecorationUI>().RefreshDecorationButtonList(); 
    }

    public void DeleteSelectedDecoration() {
        DecorationPresets.Remove(ActiveDecorationPreset);
        ActiveDecorationPreset = null;
        ToggleDecorationVariantEditorMenu(false);
        FindAnyObjectByType<DecorationUI>().RefreshDecorationButtonList();
    }

    /* Variant Logic works only when there is a decoration selected */
    public void SpawnVariant(DecorationVariant variant) {
        AddInstantiedDecorationToList(ActiveDecorationPreset.Spawn(DecorationSpawnPos, variant));
        ToggleDecorationVariantEditorMenu(false);
        FindAnyObjectByType<DecorationUI>().RefreshInstantiatedList();
    }

    public void RenameVariant(DecorationVariant variant, string newName) {
        variant.SetName(UniqueNameEnsure(newName, DecorationEnsureType.Variation));
        FindAnyObjectByType<DecorationUI>().RefreshInstantiatedList();
        ToggleDecorationVariantEditorMenu(true);
    }

    public void DeleteVariant(DecorationVariant variant) {
        ActiveDecorationPreset.Variants.Remove(variant);
        ToggleDecorationVariantEditorMenu(true);
    }

    public void RenameInstantiated(DecorationInstantiated deco, string newName) {
        deco.SetName(UniqueNameEnsure(newName, DecorationEnsureType.Instantiated));
        FindAnyObjectByType<DecorationUI>().RefreshInstantiatedList();
    }

    public void DeleteInstantiated(DecorationInstantiated deco) {
        DecorationsInstantiated.Remove(deco);
        Destroy(deco.gameObject);
        FindAnyObjectByType<DecorationUI>().RefreshInstantiatedList();
    }

    public bool DecorationPresetNameExists(string name) {
        int count = DecorationPresets.Count(decorationPreset => decorationPreset.Name == name);
        return count >= 2;
    }

    public enum DecorationEnsureType {
        Preset,
        Variation,
        Instantiated
    }

    string UniqueNameEnsure(string name, DecorationEnsureType type) {
        string baseName = name;
        string uniqueName = baseName;
        int copyNumber = 1;

        bool NameExists(string checkName) {
            switch (type) {
                case DecorationEnsureType.Preset:
                    return DecorationPresets.Any(dp => dp.Name == checkName);
                case DecorationEnsureType.Variation:
                    return ActiveDecorationPreset.Variants.Any(variant => variant.Name == checkName);
                case DecorationEnsureType.Instantiated:
                    return DecorationsInstantiated.Any(di => di.Name == checkName);
                default:
                    return false;
            }
        }

        if (!NameExists(uniqueName)) {
            return uniqueName;
        }

        while (NameExists(uniqueName)) {
            int lastIndexOfOpenParenthesis = baseName.LastIndexOf('(');
            int lastIndexOfCloseParenthesis = baseName.LastIndexOf(')');
            if (lastIndexOfOpenParenthesis != -1 && lastIndexOfCloseParenthesis == baseName.Length - 1) {
                string suffix = baseName.Substring(lastIndexOfOpenParenthesis + 1, lastIndexOfCloseParenthesis - lastIndexOfOpenParenthesis - 1);
                if (int.TryParse(suffix, out int existingNumber)) {
                    copyNumber = existingNumber + 1;
                    baseName = baseName.Substring(0, lastIndexOfOpenParenthesis).Trim();
                }
            }
            uniqueName = $"{baseName} ({copyNumber})";
            copyNumber++;
        }
        return uniqueName;
    }

    public void SetActiveDecorationPreset(DecorationPreset decoration) {
        ActiveDecorationPreset = decoration;
    }






    void AddDecorationPreset(DecorationPreset decoPreset) {
       DecorationPresets.Add(decoPreset);
    //    FindAnyObjectByType<EditorHUDui>().AddDecorationPrefabButton(decoPreset);
    }

    public void UploadNewDecorationVariant(string name, ModelAsset modelAss) {
        ActiveDecorationPreset.AddVariant(name, modelAss);
    }
    /*
    public void SpawnActiveDecoration() {
        GameObject deco = ActiveDecorationPreset.Spawn(DecorationSpawnPos);
        SpawnDecoration(deco);
    }*/

    void AddInstantiedDecorationToList(GameObject DecorationGameObject) {
        DecorationsInstantiated.Add(DecorationGameObject.GetComponent<DecorationInstantiated>());
        // Update the editor hud scrollview
     //   FindAnyObjectByType<EditorHUDui>().AddDecorationInSceneButton(DecorationGameObject);
    }

    

    public DecorationPreset GetActiveDecorationPreset() {
        return ActiveDecorationPreset;
    }

    public List<DecorationInstantiated> GetInstantiatedDecorationList() {
        return DecorationsInstantiated;
    }

    public List<DecorationPreset> GetDecorationsList() {
        return DecorationPresets;
    }
}
