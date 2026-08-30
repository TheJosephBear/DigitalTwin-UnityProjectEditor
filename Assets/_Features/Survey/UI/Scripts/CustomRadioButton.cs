using UnityEngine.UIElements;

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
        get => _toggle.value;
        set => _toggle.value = value;
    }

    [UxmlAttribute]
    public bool Multiline {
        get => _labelTextField.multiline;
        set => _labelTextField.multiline = value;
    }

    private readonly TextField _labelTextField;
    private readonly Toggle _toggle;
    public Toggle Radio => _toggle;

    public CustomRadioButton() {
        this.style.flexDirection = FlexDirection.Row;

        _toggle = new Toggle();
        _toggle.text = string.Empty;
        Add(_toggle);

        _labelTextField = new TextField();
        _labelTextField.textEdition.placeholder = "Option text";
        _labelTextField.style.flexGrow = 1;
        _labelTextField.multiline = true;
        _labelTextField.style.whiteSpace = WhiteSpace.Normal;

        var textFieldWrapper = new VisualElement();
        textFieldWrapper.style.flexGrow = 1;
        textFieldWrapper.Add(_labelTextField);
        Add(textFieldWrapper);
    }

    public void RegisterRadioCallback(EventCallback<ChangeEvent<bool>> callback) {
        _toggle.RegisterValueChangedCallback(callback);
    }
}


[UxmlElement]
public partial class CustomRadioButtonNoText : VisualElement {

    [UxmlAttribute]
    public bool @Checked
    {
        get => _toggle.value;
        set => _toggle.value = value;
    }

    private readonly Toggle _toggle;
    public Toggle Radio => _toggle;

    public CustomRadioButtonNoText() {
        this.style.flexDirection = FlexDirection.Row;

        _toggle = new Toggle();
        _toggle.text = string.Empty;
        Add(_toggle);
    }

    public void RegisterRadioCallback(EventCallback<ChangeEvent<bool>> callback) {
        _toggle.RegisterValueChangedCallback(callback);
    }
}
