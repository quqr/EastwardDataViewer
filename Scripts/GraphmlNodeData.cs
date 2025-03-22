using Godot;

namespace EastwardRipping.Scripts;

public partial class GraphmlNodeData : GraphNode
{
	public string NodeId { get; set; }
	[Export] public Label Id;
	[Export] public Label DataName;
	[Export] public Label Children;
}