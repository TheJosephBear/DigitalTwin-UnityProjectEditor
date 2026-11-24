using TMPro;
using UnityEngine;

public class LoadingScreenUI : UIBehaviour {

    [Tooltip("What is being loaded right now, set through SetStatus")]
    [SerializeField] TMP_Text _statusText;

    static LoadingScreenUI _instance;

    protected override void Awake() {
        base.Awake();
        _instance = this;
        SetText("");
    }

    /// <summary>
    /// Shows what is being loaded under the spinner, with a percentage when <paramref name="progress01"/> is 0-1.
    /// WebGL is single threaded: the text only reaches the screen once the caller lets a frame pass.
    /// </summary>
    public static void SetStatus(string text, float progress01 = -1f) {
        if (_instance == null) return;
        if (progress01 >= 0f) text += $" – {Mathf.FloorToInt(Mathf.Clamp01(progress01) * 100f)} %";
        _instance.SetText(text);
    }

    public static bool IsVisible => _instance != null && _instance.GetComponent<Canvas>().enabled;

    public override void Hide() {
        base.Hide();
        SetText("");
    }

    void SetText(string text) {
        if (_statusText != null) _statusText.text = text;
    }
}
