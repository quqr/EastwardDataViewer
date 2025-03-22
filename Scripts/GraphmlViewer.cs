using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace EastwardRipping.Scripts;

public partial class GraphmlViewer : Control
{
    [Export] GraphEdit       graphEdit;
    [Export] GraphmlNodeData nodeData;
    private  string          path = "res://graphml/Ch1_Main.quest.json";

    public override void _Ready()
    {
        graphEdit.ConnectionRequest += OnConnectionRequest;
        using var json   = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var       mlData = JsonConvert.DeserializeObject<GraphmlData>(json.GetAsText());
        var       nodes  = new List<GraphmlNodeData>();
        foreach (var node in mlData.Nodes)
        {
            CreateNode(node, nodes);
        }
        
        foreach (var connection in mlData.Connections)
        {
            var fromNode = nodes.Find(n => n.NodeId == connection.From);
            var toNode   = nodes.Find(n => n.NodeId == connection.To);
            graphEdit.ConnectNode(fromNode.Name, 0, toNode.Name, 0);
        }
    }

    private GraphmlNodeData CreateNode(EWGraphmlNodeData node, List<GraphmlNodeData> nodes)
    {
        var nodeInstance = nodeData.Duplicate() as GraphmlNodeData;
        nodeInstance.Title         = node.Fullname;
        nodeInstance.NodeId        = node.Id;
        nodeInstance.Id.Text       = node.Id;
        nodeInstance.DataName.Text = node.Name;
        nodeInstance.Children.Text = node.Children.Count.ToString();
        
        graphEdit.AddChild(nodeInstance);
        nodes.Add(nodeInstance);
        foreach (var child in node.Children)
        {
            CreateNode(child, nodes);
        }
        return nodeInstance;
    }

    private void OnConnectionRequest(StringName fromnode, long fromport, StringName tonode, long toport)
    {
        graphEdit.ConnectNode(fromnode, (int)fromport, tonode, (int)toport);
    }
}