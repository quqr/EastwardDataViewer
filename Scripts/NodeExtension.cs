using Godot;

namespace EastwardRipping.Scripts;

public static class NodeExtension
{
    public static void RemoveAllChildren(this Node node)
    {
        var children = node.GetChildren();
        foreach (var child in children)
        {
            node.RemoveChild(child);
            child.Dispose();
        }
    }
}