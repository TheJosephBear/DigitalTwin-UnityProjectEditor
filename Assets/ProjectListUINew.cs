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

        StartCoroutine(RefreshCoroutine(() => {
            ReorderUIButtonsAlphabetically();
            onCompleted();
        }));
    }
    private IEnumerator RefreshCoroutine(System.Action onCompleted) {
        UIManager.Instance.ShowUI(UIType.LoadingScreen);
        ProjectListManager.Instance.GetProjectMetadataList(list => {
            foreach (ProjectMetadata project in list) {
                ImageManager.Instance.DownloadPreviewImage(project.projectName, success => {
                    print($"Refresh called. Downloading preview image for: {project.projectName}, success: {success}");
                    TextureAsset previewAsset = null;
                    if (success) {
                        previewAsset = ImageManager.Instance.GetPreviewAssetByProject(project.projectName);
                    }
                    AddProjectButtonToList().Initialize(project: project, UIScript: this, textureAsset: previewAsset);
                    onCompleted();
                });
            }

            UIManager.Instance.HideUI(UIType.LoadingScreen);
            Debug.Log("All project dashboard preview downloads have completed processing!");
        });
        yield return null;
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
        ImageManager.Instance.AskForImageDialog((texture) => {
            EditingImage.texture = texture.Texture;
            _temporaryUploadTexture = texture;
            UpdateAspectRatio(EditingImage);
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
        TextureAsset textureAsset = ImageManager.Instance.GetPreviewAssetByProject(_projectInModal.projectName);
        if (textureAsset != null) ClassicImage.texture = textureAsset.Texture;
        UpdateAspectRatio(ClassicImage);

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

        // Image
        TextureAsset textureAsset = ImageManager.Instance.GetPreviewAssetByProject(_projectInModal.projectName);
        if (textureAsset != null) EditingImage.texture = textureAsset.Texture;
        UpdateAspectRatio(EditingImage);
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
