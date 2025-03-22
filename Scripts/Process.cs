using Godot;

namespace EastwardRipping.Scripts;

public partial class Process : Button
{
    [Export] public Animations      Anim;
    [Export] public VBoxContainer   Container;
    [Export] public PackedScene     LabelScene;
    [Export] public ScrollContainer Scroll;

    public override void _Ready()
    {
        ButtonDown += OnPressed;
    }

    private void OnPressed()
    {
        Anim.Run();
    }

    public Info AddInfo(string text)
    {
        var info = LabelScene.Instantiate<Info>();
        info.Init(text);
        Container.AddChild(info);
        Scroll.SetVScroll(Scroll.GetVScroll() + 100);
        return info;
    }
}