using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VariantButton : MonoBehaviour {
    
    int variantIndex;
    Button button;
    [SerializeField] private TextMeshProUGUI text;
    bool selected = false;

    private void Awake() {
        button = GetComponent<Button>();
    }

    public void Initialize(int variIndex) {
        variantIndex = variIndex;
        ModelAsset variantAsset = DecorationManager.Instance.GetActiveDecorationPreset().Variants[variantIndex];
        button.onClick.AddListener(OnButtonClick);
  //      button.onClick.AddListener(() => FindAnyObjectByType<DecorationUI>().RefreshVariantButtonListSelection()); // Nechuárna ale nevím jak jinak aktualizovat ty outline všech tlaèítek
        text.text = variantAsset.name;
    }

    void OnButtonClick() {
        AudioManager.Instance.PlaySound(SoundType.click);
    }

    public void onPridatDoSceny() {
        //   GetSelected();
        //  DecorationManager.Instance.ShowDecorationVariantEditorMenu();
        DecorationManager.Instance.SpawnVariant(variantIndex);
    }

    public void onPrejmenovat() {
      //  GetSelected();
        PopUpTextInput.Instance.AskForInput("Pøejmenovat dekoraci", (input) => {
       //     if (input != null)
              //  DecorationManager.Instance.RenameSelectedDecoration(input);
        });
    }

    public void onOdstranit() {
     //   GetSelected();
     //   DecorationManager.Instance.DeleteSelectedDecoration();
    }
}
