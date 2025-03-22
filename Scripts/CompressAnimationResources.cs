using System.IO;
using System.Threading.Tasks;
using Godot;

namespace EastwardRipping.Scripts;

public partial class CompressAnimationResources : Node
{
    private float _speed = 1 / 60f;

    public override void _Ready()
    {
        var absPath = "G:/Games/EastWardRip/anim/";
        var files   = Directory.GetFiles("anim", "*.tres", SearchOption.AllDirectories);
        Parallel.ForEach(files, file =>
        {
            using var sf         = ResourceLoader.Load<SpriteFrames>(file);
            var       animations = sf.GetAnimationNames();
            foreach (var animation in animations) sf.SetAnimationSpeed(animation, _speed);
            ResourceSaver.Save(sf, file, ResourceSaver.SaverFlags.Compress);
            GD.Print($"Saving {file}");
        });
        GD.Print("OK!");
    }
}