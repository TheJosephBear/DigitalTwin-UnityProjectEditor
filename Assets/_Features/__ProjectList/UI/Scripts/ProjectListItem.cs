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
        if (TextureAsset != null) EditingImage.texture = TextureAsset.Texture;
    }

    public void OnClick() {
        string id = "";
        if (TextureAsset != null) id = TextureAsset.ID;
        _projectListUI.OnProjectClick(ProjectMetadata, id);
    }
}
