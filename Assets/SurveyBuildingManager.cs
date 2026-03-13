using SurveySystem;
using UnityEngine;

public class SurveyBuildingManager : MonoBehaviour {

    public GameObject SurveyBuildingUIPrefab;

    SurveyBuilder _builderInstance;
    SurveyBuildingUI _uiInstance;

    void Start() {

    }

    public void EnterSurveyBuilding() {
        if(_builderInstance == null) _builderInstance = new SurveyBuilder();
        if(_uiInstance == null) _uiInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(SurveyBuildingUIPrefab).GetComponent<SurveyBuildingUI>();

        if (!_builderInstance.HasActiveSurvey()) _builderInstance.CreateNewSurvey();
        _uiInstance.Initialize(_builderInstance, this);

        _uiInstance.gameObject.SetActive(true);
    }

    public void SaveSurvey() {
        // Send this to the server or something
        _builderInstance.ExportSurveyAsJson();
    }

    // Disable UI and other related objects
    public void ExitSurveyCreation() {
        //   _uiInstance.gameObject.SetActive(false); // Buggy, dont know uitoolkit well enough to know why, lets just destroy it (not that expensive on one object hopefully)
        Destroy(_uiInstance.gameObject);

        // Survey manager handles calling state change
        SurveyManager.Instance.ExitSurveyBuilding();
    }
}
