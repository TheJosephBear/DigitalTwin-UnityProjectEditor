using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CustomToggleButton : VisualElement {
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
    public bool @Checked { // with the @ symbol since checked is a C# keyword
        get => _toggleButton.value;
        set => _toggleButton.value = value;
    }

    [UxmlAttribute]
    public bool Multiline {
        get => _labelTextField.multiline;
        set => _labelTextField.multiline = value;
    }

    private readonly TextField _labelTextField;
    private readonly Toggle _toggleButton;
    public Toggle Toggle => _toggleButton;

    public CustomToggleButton() {
        this.style.flexDirection = FlexDirection.Row;

        _toggleButton = new Toggle();
        _toggleButton.text = string.Empty;
        Add(_toggleButton);

        _labelTextField = new TextField();
        _labelTextField.textEdition.placeholder = "Option text";
        _labelTextField.style.flexGrow = 1;
        _labelTextField.multiline = true;
        _labelTextField.style.whiteSpace = WhiteSpace.Normal;

        var textFieldWrapper = new VisualElement(); // Wrapper to allow the TextField to grow properly (fixes element overflow to other elements)
        textFieldWrapper.style.flexGrow = 1;
        textFieldWrapper.Add(_labelTextField);
        Add(textFieldWrapper);

        this.RegisterCallback<ClickEvent>(evt => {
            if (evt.target is TextField || (evt.target as VisualElement)?.GetFirstAncestorOfType<TextField>() != null) {
                return;
            }
            if (evt.target != _toggleButton && (evt.target as VisualElement)?.GetFirstAncestorOfType<Toggle>() != _toggleButton) {
                _toggleButton.value = !_toggleButton.value;
            }
        });
    }

    public void RegisterToggleCallback(EventCallback<ChangeEvent<bool>> callback) {
        _toggleButton.RegisterValueChangedCallback(callback);
    }
}
