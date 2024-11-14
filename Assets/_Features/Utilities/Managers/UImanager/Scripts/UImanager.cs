using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : Singleton<UImanager> {

    List<UIBehaviour> uiList = new List<UIBehaviour>();
    UIBehaviour activeUIscript;

    GraphicRaycaster graphicRaycaster;

    protected override void Awake() {
        base.Awake();
        uiList = FindObjectsOfType<UIBehaviour>().ToList();
        StartCoroutine(InitializeAndHideUI());
    }

    public void HideAllUIs() {
        foreach (var ui in uiList) {
            ui.Hide();
        }
    }

    public void ShowUI(UIType uiType) {
        foreach (UIBehaviour ui in uiList) {
            if (ui.gameObject.name == uiType.ToString()) {
                activeUIscript = ui;
                ui.Show();
                return;
            }
        }
    }

    public void HideUI(UIType uiType) {
        foreach (UIBehaviour ui in uiList) {
            if (ui.gameObject.name == uiType.ToString()) {
                activeUIscript = null;
                ui.Hide();
                return;
            }
        }
    }

    public void SetRaycasterFromLatestUI() {
        graphicRaycaster = activeUIscript.GetComponent<GraphicRaycaster>(); 
    }

    public GraphicRaycaster GetRaycaster() {
        return graphicRaycaster;
    }

    public UIBehaviour GetActiveUIscript() {
        return activeUIscript;
    }

    #region Support Functions

    IEnumerator InitializeAndHideUI() {
        // First wait for them to Awake
        yield return new WaitUntil(() => isAllUIAwaken());
        HideAllUIs();
    }

    bool isAllUIAwaken() {
        foreach (var ui in uiList) {
            if (!ui.IsSetup()) return false;
        }
        return true;
    }

    #endregion

}