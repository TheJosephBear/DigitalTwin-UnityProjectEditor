using QuestionnaireToolkit.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SurveySystem;
using System;

public class SurveyManager : Singleton<SurveyManager> {
    
    public GameObject SurveyBuildingManagerPrefab;

    SurveyBuildingManager _buildingManagerInstance;

    void Start() {

        _buildingManagerInstance = SceneLoadingManager.Instance
            .InstantiateObjectInScene(SurveyBuildingManagerPrefab)
            .GetComponent<SurveyBuildingManager>();
    }

    public void EnterSurveyBuilding() {
        _buildingManagerInstance.EnterSurveyBuilding();
    }

    public void StartSurveyRuntime() {

    }


    public void CreateNewQuestionnare() {
       
    }
    
  
}
