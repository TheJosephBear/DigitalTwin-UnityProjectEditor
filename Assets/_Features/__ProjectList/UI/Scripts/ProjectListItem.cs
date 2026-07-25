using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProjectListItem : MonoBehaviour {

    public TextMeshProUGUI ButtonText;
    public RawImage EditingImage;
    public ProjectMetadata ProjectMetadata;
    public TextureAsset TextureAsset;
    private ProjectListUINew _projectListUI;

    public void Initialize(ProjectMetadata project, ProjectListUINew UIScript, TextureAsset textureAsset) {
        _projectListUI = UIScript;
        ProjectMetadata = project;
        ButtonText.text = ProjectMetadata.projectName;
        TextureAsset = textureAsset;
        if (TextureAsset != null && TextureAsset.Texture != null) {
            EditingImage.texture = TextureAsset.Texture;
            UpdateAspectRatio(EditingImage);
        }
    }

    public void OnClick() {
        string id = "";
        if (TextureAsset != null) id = TextureAsset.ID;
        _projectListUI.OnProjectClick(ProjectMetadata, id);
    }

    private void UpdateAspectRatio(RawImage rawImage) {
        if (rawImage == null || rawImage.texture == null || rawImage.texture.height == 0) return;

        var fitter = rawImage.GetComponent<AspectRatioFitter>();
        if (fitter != null) {
            fitter.aspectRatio = (float)rawImage.texture.width / rawImage.texture.height;
        }
    }
}
