using UnityEngine;
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
    public bool @Checked // with the @ symbol since checked is a C# keyword
    {
        get => _radioButton.value;
        set => _radioButton.value = value;
    }

    private readonly TextField _labelTextField;
    private readonly UnityEngine.UIElements.RadioButton _radioButton;
    public UnityEngine.UIElements.RadioButton Radio => _radioButton;

    public CustomRadioButton() {
        this.style.flexDirection = FlexDirection.Row;

        _radioButton = new UnityEngine.UIElements.RadioButton();
        _radioButton.text = string.Empty;
        Add(_radioButton);

        _labelTextField = new TextField();
        _labelTextField.textEdition.placeholder = "Option text";
        _labelTextField.style.flexGrow = 1;
        _labelTextField.multiline = true;
        _labelTextField.style.whiteSpace = WhiteSpace.Normal;

        var textFieldWrapper = new VisualElement(); // Wrapper to allow the TextField to grow properly (fixes element overflow to other elements)
        textFieldWrapper.style.flexGrow = 1;
        textFieldWrapper.Add(_labelTextField);
        Add(textFieldWrapper);
    }

    public void RegisterRadioCallback(EventCallback<ChangeEvent<bool>> callback) {
        _radioButton.RegisterValueChangedCallback(callback);
    }
}
