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
