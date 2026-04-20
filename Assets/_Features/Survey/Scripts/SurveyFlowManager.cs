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
    SurveyUIControllerEditor _uiInstanceEditor;
    SurveyUIControllerViewer _uiInstanceViewer;

    void Start() {

    }

    public void EnterSurveyBuilding() {
        if(_builderInstance == null) _builderInstance = new SurveyBuilder();
        if(_uiInstanceEditor == null) {
            _uiInstanceEditor = SceneLoadingManager.Instance != null 
                ? SceneLoadingManager.Instance.InstantiateObjectInScene(SurveyBuildingUIPrefab).GetComponent<SurveyUIControllerEditor>()
                : Instantiate(SurveyBuildingUIPrefab).GetComponent<SurveyUIControllerEditor>();
        }

        if (!_builderInstance.HasActiveSurvey()) _builderInstance.CreateNewSurvey();
        _uiInstanceEditor.Initialize(_builderInstance, SurveyManager.Instance);

        _uiInstanceEditor.gameObject.SetActive(true);
    }

    public void EnterSurveyViewing(string surveyJson) {
        if (_builderInstance == null) _builderInstance = new SurveyBuilder();
        if (_responseManInstance == null) _responseManInstance = new SurveyResponseManager();
        if (_uiInstanceViewer == null) {
            _uiInstanceViewer = SceneLoadingManager.Instance != null
                ? SceneLoadingManager.Instance.InstantiateObjectInScene(SurveyViewingUIPrefab).GetComponent<SurveyUIControllerViewer>()
                : Instantiate(SurveyViewingUIPrefab).GetComponent<SurveyUIControllerViewer>();
        }

        if (!_builderInstance.HasActiveSurvey()) _builderInstance.CreateNewSurvey(); 
        _builderInstance.DeserializeFromJson(surveyJson);
        _responseManInstance?.Initialize(_builderInstance.GetActiveSurvey());

        _uiInstanceViewer.Initialize(_builderInstance, _responseManInstance, SurveyManager.Instance);
        _uiInstanceViewer.gameObject.SetActive(true);
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
        _uiInstanceEditor.DeserializeUI();
    }

    // Disable UI and other related objects
    public void ExitSurvey() {
        //   _uiInstance.gameObject.SetActive(false); // Buggy, dont know uitoolkit well enough to know why, lets just destroy it (not that expensive on one object hopefully)
        if (_uiInstanceEditor != null) Destroy(_uiInstanceEditor.gameObject);
        if(_uiInstanceViewer != null) Destroy(_uiInstanceViewer.gameObject);
    }
}
