using UnityEngine;

public class ViewerHUDUI : UIBehaviour {

    public void OnSurvey() {
        MainManagerBase.Instance.ChangeState(ProjectState.Survey);
    }

    public void OnMultiView() {

    }

}
