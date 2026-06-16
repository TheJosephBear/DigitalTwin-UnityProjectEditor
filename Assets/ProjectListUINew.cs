using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProjectListUINew : MonoBehaviour {

    public GameObject ModalClassic;
    public GameObject ModalEditing;
    public GameObject ProjectScrollViewContent;
    public GameObject ProjectButtonPrefab;

    // Classic modal references
    public TextMeshProUGUI ClassicNameRef;
    public TextMeshProUGUI ClassicDescriptionRef;
    public RawImage ClassicImage;

    // Editing modal references
    public TMP_InputField EditingNameInput;
    public TMP_InputField EditingDescriptionInput;
    public RawImage EditingImage;

    private ProjectMetadata _projectInModal;
    private string _pendingImageID;
    private List<ProjectListItem> _projectButtons = new List<ProjectListItem>();
    private Texture _placeholderTexture;
    private TextureAsset _temporaryUploadTexture; // Stored while editing in case you dont want to use the picture

    void Awake() {

    }

    public void Initialize() {
        RefreshProjectList(() => { OnCloseModal(); });
        _placeholderTexture = ClassicImage.texture;
    }


    public void RefreshProjectList(System.Action onCompleted) {
        // Clear old buttons
        foreach (var button in _projectButtons) {
            Destroy(button.gameObject);
        }
        _projectButtons.Clear();

        StartCoroutine(RefreshCoroutine(() => { onCompleted(); }));
    }
    private IEnumerator RefreshCoroutine(System.Action onCompleted) {
        ProjectListManager.Instance.GetProjectMetadataList(list => {
            foreach (ProjectMetadata project in list) {
                ImageManager.Instance.DownloadPreviewImage(project.projectName, success => {
                    TextureAsset previewAsset = null;
                    if (success) {
                        previewAsset = ImageManager.Instance.GetPreviewAssetByID(project.projectImageID);
                    }
                    AddProjectButtonToList().Initialize(project: project, UIScript: this, textureAsset: previewAsset);
                    onCompleted();
                });
            }

            Debug.Log("All project dashboard preview downloads have completed processing!");
        });
        yield return null;
    }

    /// <summary>
    /// Create a button for each project and add it to the scroll view
    /// </summary>
    /// <returns> The project list button class instance </returns>
    ProjectListItem AddProjectButtonToList() {
        GameObject projectButtonGO = Instantiate(ProjectButtonPrefab, ProjectScrollViewContent.transform);
        ProjectListItem projectButtonScript = projectButtonGO.GetComponent<ProjectListItem>();
        _projectButtons.Add(projectButtonScript);
        return projectButtonScript;
    }



    #region Button onclicks

    #region Main UI Buttons

    public void OnCreateNewProject() {
        ProjectListManager.Instance.CreateNewProject();
    }

    // Open classic modal
    public void OnProjectClick(ProjectMetadata projectMedata, string textureID) {
        ModalClassic.SetActive(true);
        ModalEditing.SetActive(false);
        _projectInModal = projectMedata;
        _pendingImageID = textureID;
        InitializeClassicPanel();
    }

    // Open editing modal
    public void OnEditProject() {
        ModalClassic.SetActive(false);
        ModalEditing.SetActive(true);
        InitializeEditingPanel();
    }

    public void OnCloseModal() {
        ModalClassic.SetActive(false);
        ModalEditing.SetActive(false);
        _projectInModal = null;
        _pendingImageID = null;
        ClassicImage.texture = _placeholderTexture;
        EditingImage.texture = _placeholderTexture;
    }

    public void OnUserProfile() {

    }

    #endregion

    #region Classic Modal Buttons

    public void OnOpenProject() {
        ProjectListManager.Instance.OpenProject(_projectInModal);
    }

    public void OnRenameProject(string text) {
        ProjectListManager.Instance.RenameProject(_projectInModal, text);
    }

    public void OnDuplicateProject() {
        ProjectListManager.Instance.DuplicateProject(_projectInModal);
    }

    public void OnExportProject() {
        ProjectListManager.Instance.ExportProject(_projectInModal);
    }

    public void OnShowFeedBack(ProjectMetadata projectMedata) {
        ProjectListManager.Instance.ShowFeedBack(projectMedata);
    }

    public void OnDeleteProject() {
        PopUp.Instance.AreYouSurePopUp((continuing) => {
            if (continuing) {
                ProjectListManager.Instance.DeleteProject(_projectInModal);
                OnCloseModal();
            } else {

            }
        },
        text: "Chcete smazat projekt?");
    }

    public void OnLogout() {
        UIManager.Instance.HideUI(UIType.ProjectsList);
        AuthorizationManager.Instance.Logout();
    }

    #endregion

    #region Editing Modal Buttons
    
    public void OnImageUpload() {
        ImageManager.Instance.AskForImageDialog((texture) => {
            EditingImage.texture = texture.Texture;
            _temporaryUploadTexture = texture;
         //   _pendingImageID = texture.ID;
        });
    }

    public void EditProjectInfo() {
        string projectID = _projectInModal.projectId;
        ImageManager.Instance.SetAssetAsProjectPreview(_temporaryUploadTexture, _projectInModal.projectName);
        ImageManager.Instance.UploadPreviewToServer(_projectInModal.projectName);
        ProjectListManager.Instance.EditProject(
            _projectInModal.projectName,
            EditingNameInput.text,
            EditingDescriptionInput.text,
            _pendingImageID,
            () => {
                RefreshProjectList(() => {
                    OnCloseModal();
                    OnProjectClick(ProjectManager.Instance.GetProjectMetadataList().Find(x => x.projectId == projectID), _pendingImageID);
                });
            }
        );
    }

    public void CancelEditing() {
        OnProjectClick(_projectInModal, "");
    }

    #endregion

    #endregion

    void InitializeClassicPanel() {
        // Name
        ClassicNameRef.text = _projectInModal.projectName;

        // Description
        ClassicDescriptionRef.text = _projectInModal.projectDescription;

        // Image
        TextureAsset textureAsset = ImageManager.Instance.GetPreviewAssetByID(_projectInModal.projectImageID);
        print(textureAsset.name);
        print(textureAsset.Texture);
        if (textureAsset != null) ClassicImage.texture = textureAsset.Texture;
    }

    void InitializeEditingPanel() {
        _pendingImageID = _projectInModal.projectImageID;

        // Name
        EditingNameInput.text = _projectInModal.projectName;

        // Description
        EditingDescriptionInput.text = _projectInModal.projectDescription;

        // Image
        TextureAsset textureAsset = ImageManager.Instance.GetPreviewAssetByID(_projectInModal.projectImageID);
        print(textureAsset.name);
        print(textureAsset.Texture);
        if (textureAsset != null) EditingImage.texture = textureAsset.Texture;
    }

}
