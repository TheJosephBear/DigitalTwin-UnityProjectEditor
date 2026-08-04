using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager> {

    List<UIBehaviour> _uiList = new List<UIBehaviour>();
    UIBehaviour _activeUIscript;
    GraphicRaycaster _graphicRaycaster;

    protected override void Awake() {
        base.Awake();
        _uiList = FindObjectsByType<UIBehaviour>(sortMode: FindObjectsSortMode.None).ToList();
        StartCoroutine(InitializeAndHideUI());
    }

    public void HideAllUIs() {
        foreach (var ui in _uiList) {
            ui.Hide();
        }
    }

    public void ShowUI(UIType uiType) {
        foreach (UIBehaviour ui in _uiList) {
            if (ui.gameObject.name == uiType.ToString()) {
          //      print("UI manager is showing ui " + uiType);
                _activeUIscript = ui;
                ui.Show();
                return;
            }
        }
    }

    public void HideUI(UIType uiType) {
        foreach (UIBehaviour ui in _uiList) {
            if (ui.gameObject.name == uiType.ToString()) {
                _activeUIscript = null;
         //       print("UI manager is HIDING ui " + uiType);
                ui.Hide();
                return;
            }
        }
    }

    public void ToggleUI(UIType uiType, bool toggleOn) {
     //   print("UI manager is TOGGLING ui " + uiType + " " + toggleOn);
        if (toggleOn) {
            ShowUI(uiType);
        } else {
            HideUI(uiType);
        }
    }

    public void SetRaycasterFromLatestUI() {
        _graphicRaycaster = _activeUIscript.GetComponent<GraphicRaycaster>();
    }

    public GraphicRaycaster GetRaycaster() {
        return _graphicRaycaster;
    }

    public UIBehaviour GetActiveUIscript() {
        return _activeUIscript;
    }

    #region Support Functions

    IEnumerator InitializeAndHideUI() {
        // First wait for them to Awake
        yield return new WaitUntil(() => isAllUIAwaken());
        HideAllUIs();
    }

    bool isAllUIAwaken() {
        foreach (var ui in _uiList) {
            if (!ui.IsSetup()) return false;
        }
        return true;
    }

    #endregion

}
