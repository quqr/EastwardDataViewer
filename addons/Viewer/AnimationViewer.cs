#if TOOLS
using Godot;

namespace EastwardRipping.addons.Viewer;

[Tool]
public partial class AnimationViewer : EditorPlugin
{
    Control ui;
    public override void _EnterTree()
    {
         ui = ResourceLoader
                .Load<PackedScene>("res://Scene/Viewer.tscn")
                .Instantiate<Control>();
        AddControlToBottomPanel(ui, "AnimationViewer");
    }

    public override void _ExitTree()
    {
        RemoveControlFromBottomPanel(ui);
        ui.QueueFree();
    }
}
#endif