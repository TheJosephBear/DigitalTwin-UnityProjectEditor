using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;

public class DecorationUI : UIBehaviour {

    public GameObject canvas;
    public GameObject DecorationListButtonPrefab; // scrollview button prefab
    public GameObject DecorationListScrollview; // Scrollview showing decoration list in UI
    public List<DecorationButton> DecorationListButtons = new List<DecorationButton>();

    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.HideUI(UIType.DecorationMain);
    }

    public void onAddNewDecoration() {
        AudioManager.Instance.PlaySound(SoundType.click);
        DecorationPreset newDeco = DecorationManager.Instance.CreateNewDecorationPreset();
        DecorationButton newButton = AddButtonToDecorationList(newDeco);
        newButton.GetSelected();
        RefreshDecorationButtonList();
        onNastaveni();
    }

    public void onNastaveni() {
        UImanager.Instance.ShowUI(UIType.DecorationPopUp);
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

    


    public override void Show() {
        canvas.SetActive(true);
        RefreshDecorationButtonList();
    }

    public override void Hide() {
        canvas.SetActive(false);
    }
}
