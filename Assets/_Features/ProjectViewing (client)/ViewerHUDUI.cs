using UnityEngine;

public class ViewerHUDUI : UIBehaviour {

    public void OnSurvey() {
        MainManagerBase.Instance.ChangeState(AppState.Survey);
    }

    public void OnMultiView() {

    }

}
