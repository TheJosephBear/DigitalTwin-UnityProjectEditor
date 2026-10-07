using UnityEngine;

public class ViewerHUDUI : UIBehaviour {

    public GameObject SurveyButton;
    public GameObject MultiviewButton;

    public void DisableUnneededButtons() {
        SurveyManager.Instance?.CheckHasValidSurvey((result) => {
            if (!result && SurveyButton != null) {
                SurveyButton.SetActive(false);
            }
        });

        print("Disabling...");
        print("Disabling...");
        print("Disabling...");
        print($"MapManager.Instance {MapManager.Instance}");
        print($"!MapManager.Instance.HasVariant() {!MapManager.Instance.HasVariant()}");
        print($"MultiviewButton != null {MultiviewButton != null}"); // THIS IS FALSE SOMEHOW EVEN THOUGH ITS ADDED IN INSPECTOR
        if (MapManager.Instance != null && !MapManager.Instance.HasVariant()) {
            if (MultiviewButton != null) {
                MultiviewButton.SetActive(false);
            }
        }
    }

    public void OnSurvey() {
        MainManagerBase.Instance.ChangeState(AppState.Survey);
    }

    public void OnMultiView() {

    }

}
