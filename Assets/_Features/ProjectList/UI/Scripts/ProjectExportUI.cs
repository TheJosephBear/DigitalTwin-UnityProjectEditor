using TMPro;
using UnityEngine;

public class ProjectExportUI : MonoBehaviour {

    public TMP_InputField IframeTextFieldReff;
    public TMP_InputField URLTextFieldReff;

    public void FillTextFields(string iframeString, string urlString) {
        IframeTextFieldReff.text = iframeString;
        URLTextFieldReff.text = urlString;
    }

    public void CloseWindow() {
        Destroy(this.gameObject);
    }

}
