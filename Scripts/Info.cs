using Godot;

namespace EastwardRipping.Scripts;

public partial class Info : HBoxContainer
{
    [Export] public CheckBox CheckInfo;
    [Export] public Label    InfoLabel;

    public void Init(string info)
    {
        InfoLabel.Text = info;
    }

    public void SetCheck(bool value)
    {
        CheckInfo.SetPressed(value);
    }
}