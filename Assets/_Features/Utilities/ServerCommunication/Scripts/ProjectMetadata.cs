using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProjectMetadata {
    /// <summary>
    /// Necessary information to show project in project list.
    /// Refference for getting all project data from the web.
    /// </summary>

    public string projectId;
    public string projectName;
    public string projectDescription;
    public string projectImageID;
    public string owner;
    public bool hasSurvey;
    public int respondentCount;

    /// <summary>
    /// Returns formatted Czech string for respondent count:
    /// 1 respondent, 2-4 respondenti, 0 nebo 5+ respondentů.
    /// </summary>
    public static string GetRespondentCountCzechText(int count) {
        if (count == 1) return "1 respondent";
        if (count >= 2 && count <= 4) return $"{count} respondenti";
        return $"{count} respondentů";
    }

    /// <summary>
    /// Returns formatted Czech string for survey status:
    /// "Aktivní" / "Bez dotazníku".
    /// </summary>
    public static string GetSurveyStatusCzechText(bool hasSurvey) {
        return hasSurvey ? "Aktivní" : "Bez dotazníku";
    }
}
