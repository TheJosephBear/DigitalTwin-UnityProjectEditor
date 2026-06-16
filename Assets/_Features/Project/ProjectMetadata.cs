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

}
