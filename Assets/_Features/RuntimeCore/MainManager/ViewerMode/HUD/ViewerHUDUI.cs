using UnityEngine;

public class ViewerHUDUI : UIBehaviour, IProjectSettingsListener {

    public GameObject SurveyButton;
    public GameObject MultiviewButton;
    public GameObject FPSButton;

    public void OnSettingsChanged(ProjectSettingsSerializable settings) {
        MultiviewButton.SetActive(settings.multiviewAllowed);
        FPSButton.SetActive(settings.firstPersonAllowed);
        SurveyButton.SetActive(settings.surveyAllowed);
    }

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

    private void OnEnable() {
        ProjectSettingsManager.Instance.AddObserver(this);
    }

    private void OnDisable() {
        ProjectSettingsManager.Instance.AddObserver(this);
    }

}
