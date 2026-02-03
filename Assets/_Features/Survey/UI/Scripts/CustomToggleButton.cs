using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CustomToggleButton : VisualElement
{
    [UxmlAttribute]
    public string LabelText
    {
        get => _labelTextField.value;
        set => _labelTextField.value = value;
    }

    [UxmlAttribute]
    public string Placeholder
    {
        get => _labelTextField.textEdition.placeholder;
        set => _labelTextField.textEdition.placeholder = value;
    }

    [UxmlAttribute]
    public bool @Checked // with the @ symbol since checked is a C# keyword
    {
        get => _toggleButton.value;
        set => _toggleButton.value = value;
    }

    private readonly TextField _labelTextField;
    private readonly Toggle _toggleButton;
    
    public CustomToggleButton()
    {
        this.style.flexDirection = FlexDirection.Row;

        _toggleButton = new Toggle();
        _toggleButton.text = string.Empty;
        Add(_toggleButton);
        
        _labelTextField = new TextField();
        _labelTextField.textEdition.placeholder = "Option text";
        _labelTextField.style.flexGrow = 1;
        Add(_labelTextField);
    }
}
