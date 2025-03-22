using System.Collections.Generic;

namespace EastwardRipping.Scripts;

public class GraphmlData
{
    public List<Connection>        Connections { get; set; }
    public List<EWGraphmlNodeData> Nodes       { get; set; }
}

public class Connection
{
    public bool   Cond { get; set; }
    public string From { get; set; }
    public string To   { get; set; }
    public string Type { get; set; }
}

public class EWGraphmlNodeData
{
    public List<EWGraphmlNodeData> Children   { get; set; }
    public string                  Fullname   { get; set; }
    public string                  Id         { get; set; }
    public string                  Name       { get; set; }
    public bool                    Transition { get; set; }
    public string                  Type       { get; set; }
}