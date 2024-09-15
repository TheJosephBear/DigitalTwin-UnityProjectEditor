using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;

public class DecorationUI : MonoBehaviour {

    public GameObject DecorationListButtonPrefab; // scrollview button prefab
    public GameObject DecorationListScrollview; // Scrollview showing decoration list in UI (content of scrollview)
    public List<DecorationButton> DecorationListButtons = new List<DecorationButton>();

    public void onX() {
        AudioManager.Instance.PlaySound(SoundType.click);
        UImanager.Instance.HideUI(UIType.DecorationMain);
    }

    public void onAddNewDecoration() {
        AudioManager.Instance.PlaySound(SoundType.click);
        DecorationPreset newDeco = DecorationManager.Instance.CreateNewDecorationPreset();
        DecorationButton newButton = AddButtonToDecorationList(newDeco);
        RefreshDecorationButtonList();
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

    

}
