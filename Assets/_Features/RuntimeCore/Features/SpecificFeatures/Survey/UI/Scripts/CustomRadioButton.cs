using UnityEngine.UIElements;
using UIRadioButton = UnityEngine.UIElements.RadioButton;

[UxmlElement]
public partial class CustomRadioButton : VisualElement {
    [UxmlAttribute]
    public string LabelText {
        get => _labelTextField.value;
        set => _labelTextField.value = value;
    }

    [UxmlAttribute]
    public string Placeholder {
        get => _labelTextField.textEdition.placeholder;
        set => _labelTextField.textEdition.placeholder = value;
    }

    [UxmlAttribute]
    public bool @Checked {
        get => _radioButton.value;
        set => _radioButton.value = value;
    }

    [UxmlAttribute]
    public bool Multiline {
        get => _labelTextField.multiline;
        set => _labelTextField.multiline = value;
    }

    private readonly TextField _labelTextField;
    private readonly UIRadioButton _radioButton;
    private bool _previewOnly;
    public UIRadioButton Radio => _radioButton;

    // Editor: the option only previews the answer, the admin can't check it (the text stays editable)
    public void SetPreviewOnly() {
        _previewOnly = true;
        AddToClassList(PreviewOnlyClass);
        IgnorePointer(_radioButton);
    }

    public const string PreviewOnlyClass = "option--preview-only";

    public static void IgnorePointer(VisualElement element) {
        element.pickingMode = PickingMode.Ignore;
        foreach (VisualElement child in element.Children()) {
            IgnorePointer(child);
        }
    }

    public CustomRadioButton() {
        this.style.flexDirection = FlexDirection.Row;

        _radioButton = new UIRadioButton();
        _radioButton.text = string.Empty;
        _radioButton.label = string.Empty;
        Add(_radioButton);

        _labelTextField = new TextField();
        _labelTextField.textEdition.placeholder = "Option text";
        _labelTextField.style.flexGrow = 1;
        _labelTextField.multiline = true;
        _labelTextField.style.whiteSpace = WhiteSpace.Normal;

        var textFieldWrapper = new VisualElement();
        textFieldWrapper.style.flexGrow = 1;
        textFieldWrapper.Add(_labelTextField);
        Add(textFieldWrapper);

        this.RegisterCallback<ClickEvent>(evt => {
            if (_previewOnly) return;
            if (evt.target is TextField || (evt.target as VisualElement)?.GetFirstAncestorOfType<TextField>() != null) {
                return;
            }
            if (!_radioButton.value) {
                _radioButton.value = true;
            }
        });
    }

    public void RegisterRadioCallback(EventCallback<ChangeEvent<bool>> callback) {
        _radioButton.RegisterValueChangedCallback(callback);
    }
}


[UxmlElement]
public partial class CustomRadioButtonNoText : VisualElement {

    [UxmlAttribute]
    public bool @Checked
    {
        get => _radioButton.value;
        set => _radioButton.value = value;
    }

    private readonly UIRadioButton _radioButton;
    private bool _previewOnly;
    public UIRadioButton Radio => _radioButton;

    // Editor: the grid cell only previews the answer, the admin can't check it
    public void SetPreviewOnly() {
        _previewOnly = true;
        CustomRadioButton.IgnorePointer(this);
    }

    public CustomRadioButtonNoText() {
        this.style.flexDirection = FlexDirection.Row;

        _radioButton = new UIRadioButton();
        _radioButton.text = string.Empty;
        _radioButton.label = string.Empty;
        Add(_radioButton);

        this.RegisterCallback<ClickEvent>(evt => {
            if (_previewOnly) return;
            if (!_radioButton.value) {
                _radioButton.value = true;
            }
        });
    }

    public void RegisterRadioCallback(EventCallback<ChangeEvent<bool>> callback) {
        _radioButton.RegisterValueChangedCallback(callback);
    }
}
