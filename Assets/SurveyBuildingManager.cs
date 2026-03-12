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
        _uiInstance.Initialize(_builderInstance);

        _uiInstance.gameObject.SetActive(true);
    }
}
