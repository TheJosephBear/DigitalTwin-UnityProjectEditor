using UnityEngine;

public class ViewerHUDUI : UIBehaviour {

    public GameObject SurveyButton;
    public GameObject MultiviewButton;

    public void DisableUnneededButtons() {
        SurveyManager.Instance.CheckHasValidSurvey((result) => {
            if (!result) {
                SurveyButton.SetActive(false);
            }
        });

        if (!MapManager.Instance.HasVariant()) {
            MultiviewButton.SetActive(false);
        }
    }

    public void OnSurvey() {
        MainManagerBase.Instance.ChangeState(AppState.Survey);
    }

    public void OnMultiView() {

    }

}
