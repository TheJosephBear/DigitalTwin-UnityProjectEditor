using SurveySystem;
using UnityEngine;

/// <summary>
/// Takes care of instantiating and connecting UI instance and data model 
/// </summary>
public class SurveyFlowManager : MonoBehaviour {

    public GameObject SurveyBuildingUIPrefab;
    public GameObject SurveyViewingUIPrefab;

    SurveyBuilder _builderInstance;
    SurveyResponseManager _responseManInstance;
    SurveyUIController _uiInstance;

    void Start() {

    }

    public void EnterSurveyBuilding() {
        if(_builderInstance == null) _builderInstance = new SurveyBuilder();
        if(_uiInstance == null) _uiInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(SurveyBuildingUIPrefab).GetComponent<SurveyUIController>();

        if (!_builderInstance.HasActiveSurvey()) _builderInstance.CreateNewSurvey();
        _uiInstance.Initialize(_builderInstance, SurveyManager.Instance);

        _uiInstance.gameObject.SetActive(true);
    }

    public void EnterSurveyViewing() {
        if (_builderInstance == null) _builderInstance = new SurveyBuilder();
        if (_responseManInstance == null) _responseManInstance = new SurveyResponseManager();
        if (_uiInstance == null) {
            _uiInstance = SceneLoadingManager.Instance.InstantiateObjectInScene(SurveyViewingUIPrefab).GetComponent<SurveyUIController>();
        }

        if (!_builderInstance.HasActiveSurvey()) _builderInstance.CreateNewSurvey();
        _uiInstance.Initialize(_builderInstance, _responseManInstance, SurveyManager.Instance);

        _uiInstance.gameObject.SetActive(true);
    }

    public string GetSurveyJsonData() {
        return _builderInstance.ExportSurveyAsJson();
    }

    public string GetResponseJsonData() {
        return _responseManInstance.ExportResponseJson();
    }

    // Deserialize survey json into the data model, then build the UI
    public void DeserializeSurvey(string surveyJson) {
        _builderInstance.DeserializeFromJson(surveyJson);
        _responseManInstance?.Initialize(_builderInstance.GetActiveSurvey());
        _uiInstance.DeserializeUI();
    }

    // Disable UI and other related objects
    public void ExitSurvey() {
        //   _uiInstance.gameObject.SetActive(false); // Buggy, dont know uitoolkit well enough to know why, lets just destroy it (not that expensive on one object hopefully)
        Destroy(_uiInstance.gameObject);
    }
}
