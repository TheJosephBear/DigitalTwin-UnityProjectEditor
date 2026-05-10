using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImageTestUI : MonoBehaviour {

    public RawImage RawImageReff;

    void Awake() {
        SceneManager.LoadScene("Utilities", LoadSceneMode.Additive);
    }

    public void OpenFileDialog() {
        ImageManager.Instance.AskForImageDialog((texture) => {
            RawImageReff.texture = texture.Texture;
        });
    }
}
