using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Classic Modal Survey References (Assign in Prefab)")]
    public TextMeshProUGUI ClassicSurveyStatusRef;
    public TextMeshProUGUI ClassicSurveyRespondentsRef;
    public GameObject ClassicSurveySection;
    public Button ClassicDownloadResponsesButton;

    // Editing modal references
    public TMP_InputField EditingNameInput;
    public TMP_InputField EditingDescriptionInput;
    public RawImage EditingImage;

    private ProjectMetadata _projectInModal;
    private string _pendingImageID;
    private List<ProjectListItem> _projectButtons = new List<ProjectListItem>();
    private Texture _placeholderTexture;
    private PendingPreviewImage _pendingPreview; // Picked in the editing modal, uploaded only when the edit is saved

    void Awake() {

    }

    public void Initialize() {
        _placeholderTexture = ClassicImage.texture;
        RefreshProjectList(() => { OnCloseModal(); });
    }


    public void RefreshProjectList(System.Action onCompleted) {
        // Clear old buttons
        foreach (var button in _projectButtons) {
            Destroy(button.gameObject);
        }
        _projectButtons.Clear();

        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        ProjectListManager.Instance.GetProjectMetadataList(list => {
            UIManager.Instance.HideUI(UIType.LoadingScreen);
            if (list == null) {
                onCompleted?.Invoke();
                return;
            }
            MemoryLogger.Log($"Project list metadata received ({list.Count} projects)");

            ImageManager.Instance.KeepOnlyPreviews(list.Select(x => x.projectName).ToList());

            // Buttons show the cached preview right away, the fresh one replaces it once downloaded
            foreach (ProjectMetadata project in list) {
                ProjectListItem item = AddProjectButtonToList();
                item.Initialize(project: project, UIScript: this, preview: ImageManager.Instance.GetPreviewTexture(project.projectName));

                ImageManager.Instance.DownloadPreviewImage(project.projectName, preview => {
                    if (item != null) item.SetPreview(preview);
                    if (_projectInModal != null && _projectInModal.projectName == project.projectName) ShowModalPreview();
                });
            }

            ReorderUIButtonsAlphabetically();
            onCompleted?.Invoke();
        });
    }

    private void ReorderUIButtonsAlphabetically() {
        var buttons = ProjectScrollViewContent.transform.Cast<Transform>()
            .OrderBy(t => t.GetComponent<ProjectListItem>().ProjectMetadata.projectName)
            .ToList();

        for (int i = 0; i < buttons.Count; i++) {
            buttons[i].SetSiblingIndex(i);
        }
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
    public void OnProjectClick(ProjectMetadata projectMedata) {
        DiscardPendingPreview();
        ModalClassic.SetActive(true);
        ModalEditing.SetActive(false);
        _projectInModal = projectMedata;
        InitializeClassicPanel();
    }

    // Open editing modal
    public void OnEditProject() {
        ModalClassic.SetActive(false);
        ModalEditing.SetActive(true);
        InitializeEditingPanel();
    }

    public void OnCloseModal() {
        DiscardPendingPreview();
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

    public void OnDownloadSurveyResponses() {
        if (_projectInModal != null) {
            if (!_projectInModal.hasSurvey) {
                PopUp.Instance.ShowPopUpWindow("K tomuto projektu není vytvořen žádný dotazník.");
                return;
            }
            if (_projectInModal.respondentCount == 0) {
                PopUp.Instance.ShowPopUpWindow("K tomuto projektu zatím nejsou k dispozici žádné odpovědi.");
                return;
            }
            ProjectListManager.Instance.DownloadSurveyResponses(_projectInModal);
        }
    }

    public void OnDownloadSurveyResponses(ProjectMetadata projectMetadata) {
        if (projectMetadata != null) {
            if (!projectMetadata.hasSurvey) {
                PopUp.Instance.ShowPopUpWindow("K tomuto projektu není vytvořen žádný dotazník.");
                return;
            }
            if (projectMetadata.respondentCount == 0) {
                PopUp.Instance.ShowPopUpWindow("K tomuto projektu zatím nejsou k dispozici žádné odpovědi.");
                return;
            }
        }
        ProjectListManager.Instance.DownloadSurveyResponses(projectMetadata);
    }

    public void OnShowFeedBack() {
        OnDownloadSurveyResponses();
    }

    public void OnShowFeedBack(ProjectMetadata projectMedata) {
        OnDownloadSurveyResponses(projectMedata);
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
        ImageManager.Instance.AskForPreviewImageDialog(image => {
            if (image == null) return;
            if (_projectInModal == null) {
                Destroy(image.Texture);
                return;
            }

            DiscardPendingPreview();
            _pendingPreview = image;
            EditingImage.texture = image.Texture;
            UpdateAspectRatio(EditingImage);
        });
    }

    public void EditProjectInfo() {
        string projectID = _projectInModal.projectId;
        string oldName = _projectInModal.projectName;
        string newName = EditingNameInput.text.Trim();
        PendingPreviewImage preview = _pendingPreview;
        _pendingPreview = null;

        UIManager.Instance.ShowUI(UIType.LoadingScreen);

        // The preview is uploaded after the rename, so it lands in the project's final folder
        ProjectListManager.Instance.EditProject(oldName, newName, EditingDescriptionInput.text, _pendingImageID, success => {
            string projectName = success ? newName : oldName;
            if (success) ImageManager.Instance.RenamePreview(oldName, newName);

            if (preview == null) {
                ReopenProjectAfterEdit(projectID);
                return;
            }

            ImageManager.Instance.UploadPreviewImage(projectName, preview, uploaded => {
                Destroy(preview.Texture);
                ReopenProjectAfterEdit(projectID);
            });
        });
    }

    private void ReopenProjectAfterEdit(string projectID) {
        RefreshProjectList(() => {
            OnCloseModal();
            ProjectMetadata project = ProjectManager.Instance.GetProjectMetadataByID(projectID);
            if (project != null) OnProjectClick(project);
        });
    }

    public void CancelEditing() {
        OnProjectClick(_projectInModal);
    }

    private void DiscardPendingPreview() {
        if (_pendingPreview == null) return;

        if (EditingImage.texture == _pendingPreview.Texture) EditingImage.texture = _placeholderTexture;
        Destroy(_pendingPreview.Texture);
        _pendingPreview = null;
    }

    #endregion

    #endregion

    void InitializeClassicPanel() {
        // Name
        ClassicNameRef.text = _projectInModal.projectName;

        // Description
        ClassicDescriptionRef.text = _projectInModal.projectDescription;

        ShowModalPreview();

        // Survey Info & Download Responses Button
        UpdateClassicSurveyPanel();
    }

    private void UpdateClassicSurveyPanel() {
        if (_projectInModal == null) return;

        bool hasSurvey = _projectInModal.hasSurvey;
        int respCount = _projectInModal.respondentCount;

        Color activeColor = new Color(0.298f, 0.686f, 0.314f); // #4CAF50
        Color inactiveColor = new Color(0.62f, 0.62f, 0.62f);  // #9E9E9E

        if (ClassicSurveySection != null) {
            ClassicSurveySection.SetActive(true);
        }

        if (ClassicSurveyStatusRef != null) {
            ClassicSurveyStatusRef.color = hasSurvey ? activeColor : inactiveColor;
            ClassicSurveyStatusRef.text = ProjectMetadata.GetSurveyStatusCzechText(hasSurvey);
        }

        if (ClassicSurveyRespondentsRef != null) {
            if (!hasSurvey) {
                ClassicSurveyRespondentsRef.text = "-";
            } else if (respCount == 0) {
                ClassicSurveyRespondentsRef.text = "Zatím žádné odpovědi";
            } else {
                ClassicSurveyRespondentsRef.text = ProjectMetadata.GetRespondentCountCzechText(respCount);
            }
        }

        // Visual styling for download button (dimmed if no responses available)
        Button downloadBtn = ClassicDownloadResponsesButton;
        if (downloadBtn == null && ModalClassic != null) {
            var buttons = ModalClassic.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons) {
                if (b.gameObject.name == "DownloadResponsesButton") {
                    downloadBtn = b;
                    break;
                }
            }
        }

        if (downloadBtn != null) {
            bool canDownload = hasSurvey && respCount > 0;
            if (downloadBtn.targetGraphic != null) {
                Color c = downloadBtn.targetGraphic.color;
                c.a = canDownload ? 1.0f : 0.4f;
                downloadBtn.targetGraphic.color = c;
            }
            var canvasGroup = downloadBtn.GetComponent<CanvasGroup>();
            if (canvasGroup != null) {
                canvasGroup.alpha = canDownload ? 1.0f : 0.4f;
            }
        }
    }

    void InitializeEditingPanel() {
        _pendingImageID = _projectInModal.projectImageID;

        // Name
        EditingNameInput.text = _projectInModal.projectName;

        // Description
        EditingDescriptionInput.text = _projectInModal.projectDescription;

        ShowModalPreview();
    }

    // Shows the project's cached preview in both modals; the editing modal keeps a newly picked image
    private void ShowModalPreview() {
        Texture preview = ImageManager.Instance.GetPreviewTexture(_projectInModal.projectName);
        if (preview == null) preview = _placeholderTexture;

        ClassicImage.texture = preview;
        UpdateAspectRatio(ClassicImage);

        if (_pendingPreview == null) {
            EditingImage.texture = preview;
            UpdateAspectRatio(EditingImage);
        }
    }

    private void UpdateAspectRatio(RawImage rawImage) {
        if (rawImage == null || rawImage.texture == null || rawImage.texture.height == 0) return;

        var fitter = rawImage.GetComponent<AspectRatioFitter>();
        if (fitter == null) {
            fitter = rawImage.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        }

        fitter.aspectRatio = (float)rawImage.texture.width / rawImage.texture.height;
    }
}
