using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectListItem : MonoBehaviour {

    public TextMeshProUGUI ButtonText;
    public RawImage EditingImage;
    public ProjectMetadata ProjectMetadata;
    private ProjectListUINew _projectListUI;

    [Header("Survey UI References (Assign in Prefab)")]
    public TextMeshProUGUI SurveyStatusText;
    public TextMeshProUGUI SurveyRespondentsText;
    public GameObject SurveyBadge;

    public void Initialize(ProjectMetadata project, ProjectListUINew UIScript, Texture2D preview) {
        _projectListUI = UIScript;
        ProjectMetadata = project;
        ButtonText.text = ProjectMetadata.projectName;
        SetPreview(preview);

        UpdateSurveyUI();
    }

    public void SetPreview(Texture2D preview) {
        if (preview == null) return;
        EditingImage.texture = preview;
        UpdateAspectRatio(EditingImage);
    }

    private void UpdateSurveyUI() {
        if (ProjectMetadata == null) return;

        bool hasSurvey = ProjectMetadata.hasSurvey;
        int respCount = ProjectMetadata.respondentCount;

        Color activeColor = new Color(0.298f, 0.686f, 0.314f); // #4CAF50
        Color inactiveColor = new Color(0.62f, 0.62f, 0.62f);  // #9E9E9E

        // Zobrazit badge pouze v případě, že projekt má dotazník
        if (SurveyBadge != null) {
            SurveyBadge.SetActive(hasSurvey);
        }

        if (SurveyStatusText != null) {
            if (hasSurvey) {
                SurveyStatusText.color = activeColor;
                if (SurveyRespondentsText != null) {
                    SurveyStatusText.text = "Aktivní";
                } else {
                    SurveyStatusText.text = $"Aktivní · {ProjectMetadata.GetRespondentCountCzechText(respCount)}";
                }
            } else {
                SurveyStatusText.color = inactiveColor;
                SurveyStatusText.text = "○ Bez dotazníku";
            }
        }

        if (SurveyRespondentsText != null) {
            if (hasSurvey) {
                // Pokud nemáme samostatný status text, jde o kompaktní badge s ikonkou -> zobrazujeme pouze číslo
                if (SurveyStatusText == null) {
                    SurveyRespondentsText.text = respCount.ToString();
                } else {
                    SurveyRespondentsText.text = ProjectMetadata.GetRespondentCountCzechText(respCount);
                }
            } else {
                SurveyRespondentsText.text = "";
            }
        }
    }

    public void OnClick() {
        _projectListUI.OnProjectClick(ProjectMetadata);
    }

    private void UpdateAspectRatio(RawImage rawImage) {
        if (rawImage == null || rawImage.texture == null || rawImage.texture.height == 0) return;

        var fitter = rawImage.GetComponent<AspectRatioFitter>();
        if (fitter != null) {
            fitter.aspectRatio = (float)rawImage.texture.width / rawImage.texture.height;
        }
    }
}
