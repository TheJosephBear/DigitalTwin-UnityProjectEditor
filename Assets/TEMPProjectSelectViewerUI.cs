using UnityEngine;

public class TEMPProjectSelectViewerUI : MonoBehaviour {

    string _projectToSelect;
    ViewingInitializer _initializer;

    private void Awake() {
        _initializer = FindAnyObjectByType<ViewingInitializer>();
    }

    public void SetProjectToSelect(string projectName) { 
        _projectToSelect = projectName;
    }

    public void LoadProject() {
        _initializer._projectName = _projectToSelect;
        _initializer.InitializeViewer();
    }
}
