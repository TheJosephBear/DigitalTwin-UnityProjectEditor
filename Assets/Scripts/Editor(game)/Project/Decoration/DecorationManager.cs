using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationManager : Singleton<DecorationManager> {
    
    List<DecorationPreset> DecorationPresets = new List<DecorationPreset>();
    List<Decoration> DecorationsInstantiated = new List<Decoration>();
    public DecorationPreset UIactivePreset { get; private set; }
    public Vector3 DecorationSpawnPos;

    public void CreateNewDecorationPreset() {
        DecorationPreset decorationPreset = new DecorationPreset();
        decorationPreset.SetName("new Decoration");
        AddDecorationPreset(decorationPreset);
    }

    void AddDecorationPreset(DecorationPreset decoPreset) {
        DecorationPresets.Add(decoPreset);
        FindAnyObjectByType<EditorHUDui>().AddDecorationPrefabButton(decoPreset);
    }

    public void UploadNewDecorationModel(ModelAsset modelAss) {
        UIactivePreset.AddVariant(modelAss);
    }

    public void SpawnActiveDecoration() {
        GameObject deco = UIactivePreset.Spawn(DecorationSpawnPos);
        SpawnDecoration(deco);
        /*
        foreach(Decoration decor in DecorationsInstantiated) {
            print(decor.decorationPreset.Variants[decor.decorationVariantIdx].ModelID);
        }*/
    }

    void SpawnDecoration(GameObject DecorationGameObject) {
        DecorationsInstantiated.Add(DecorationGameObject.GetComponent<Decoration>());
        FindAnyObjectByType<EditorHUDui>().AddDecorationInSceneButton(DecorationGameObject);
    }

    public void SetActiveDecorationPreset(DecorationPreset decoration) {
        UIactivePreset = decoration;
    }

    public List<SerializableDecorationPreset> SerializeDecorationPresets() {
        List<SerializableDecorationPreset> serializedPresets = new List<SerializableDecorationPreset>();

        foreach (var preset in DecorationPresets) {
            SerializableDecorationPreset serializedPreset = new SerializableDecorationPreset {
                name = preset.Name,
                modelAssetIDs = new List<string>()
            };

            foreach (var variant in preset.Variants) {
                serializedPreset.modelAssetIDs.Add(variant.ModelID); 
            }

            serializedPresets.Add(serializedPreset);
        }

        return serializedPresets;
    }

    public List<SerializableDecoration> SerializeDecorationsInstantiated() {
        List<SerializableDecoration> serializedDecorations = new List<SerializableDecoration>();

        foreach (var decoration in DecorationsInstantiated) {
            SerializableDecoration serializedDecoration = new SerializableDecoration {
                decorationPresetName = decoration.decorationPreset.Name,
                modelAssetID = decoration.decorationPreset.Variants[decoration.decorationVariantIdx].ModelID,
                position = decoration.transform.position
            };

            serializedDecorations.Add(serializedDecoration);
        }

        return serializedDecorations;
    }
    public void DeserializeDecorationPresets(List<SerializableDecorationPreset> serializedPresets) {
        DecorationPresets.Clear();

        foreach (var serializedPreset in serializedPresets) {
            DecorationPreset preset = new DecorationPreset();
            preset.SetName(serializedPreset.name);

            foreach (var modelID in serializedPreset.modelAssetIDs) {
                ModelAsset modelAsset = AssetManager.Instance.FindModelAssetByID(modelID);
                preset.AddVariant(modelAsset);
            }

            AddDecorationPreset(preset);
        }
    }

    public void DeserializeDecorationsInstantiated(List<SerializableDecoration> serializedDecorations) {
        DecorationsInstantiated.Clear();

        foreach (var serializedDecoration in serializedDecorations) {
            DecorationPreset preset = FindDecorationPresetByName(serializedDecoration.decorationPresetName);
            if (preset != null) {
                int variantIdx = preset.Variants.FindIndex(v => v.ModelID == serializedDecoration.modelAssetID);
                GameObject deco = preset.Spawn(serializedDecoration.position, variantIdx);
                SpawnDecoration(deco);
            }
        }
    }

    DecorationPreset FindDecorationPresetByName(string name) {
        foreach (DecorationPreset decoPre in DecorationPresets) {
            if (decoPre.Name == name) return decoPre;
        }
        return null;
    }
}
