using System.Collections.Generic;

namespace EastwardRipping.AnimationJsonData;

public class SpriteJsonData
{
    public Dictionary<string, AnimationData> anims    { get; set; }
    public Dictionary<string, string>        atlases  { get; set; }
    public List<FeatureData>                 features { get; set; }
    public Dictionary<string, FrameData>     frames   { get; set; }
    public Dictionary<string, ModuleData>    modules  { get; set; }
}

public class ModuleData
{
    public string      atlas   { get; set; }
    public float       feature { get; set; }
    public List<float> rect    { get; set; }
}

public class FrameData
{
    public string             name  { get; set; }
    public List<List<object>> parts { get; set; }
    public MetaData           meta  { get; set; }
}

public class MetaData
{
    public AnchorData  anchors  { get; set; }
    public float       glow_sum { get; set; }
    public List<float> origin   { get; set; }
}

public class AnchorData
{
    public List<List<object>>? arm { get; set; }
}

public class FeatureData
{
    public string name { get; set; }
    public float  id   { get; set; }
}

public class AnimationData
{
    public bool               deprecated { get; set; }
    public string             name       { get; set; }
    public List<List<object>> seq        { get; set; }
    public string             src_type   { get; set; }
}