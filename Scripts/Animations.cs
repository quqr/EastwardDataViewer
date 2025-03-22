using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EastwardRipping.AnimationJsonData;
using Godot;
using Newtonsoft.Json;
using FileAccess = Godot.FileAccess;

namespace EastwardRipping.Scripts;

public partial class Animations : Node
{
    private          double     _animationSpeed = 10f;
    private readonly List<Task> _tasks          = new(150);
    [Export] public  Process    Ps;
    public           bool       Stop;

    public async void Run()
    {
        var dirs = Directory.GetDirectories("anim", "*", SearchOption.AllDirectories);
        for (var index = 0; index < dirs.Length; index++)
        {
            if (_tasks.Count >= 100)
            {
                Stop = true;
                var i = Ps.AddInfo("Wait all tasks complete...");
                GD.Print("Wait all tasks complete...");
                while (Stop)
                {
                    _tasks.RemoveAll(x => x.IsCompleted);
                    if (_tasks.Count <= 10) Stop = false;
                    await ToSignal(GetTree(), "process_frame");
                }

                i.SetCheck(true);
            }

            var info = Ps.AddInfo($"({index + 1}/{dirs.Length}) {dirs[index]} ");
            GD.Print($"({index + 1}/{dirs.Length})Processing {dirs[index]}...");
            var dir = dirs[index];
            if (!FileAccess.FileExists(Path.Combine(dir, "atlas.png"))) continue;
            if (!FileAccess.FileExists(Path.Combine(dir, "def.json"))) continue;

            using var json       = FileAccess.Open(Path.Combine(dir, "def.json"), FileAccess.ModeFlags.Read);
            using var image      = ResourceLoader.Load<Texture2D>(Path.Combine(dir, "atlas.png"));
            var       outputPath = Path.Combine(dir, $"{dir.Split(Path.DirectorySeparatorChar).Last()}.tres");

            if (json is null || image is null) continue;

            var task = Process(image, json.GetAsText(), outputPath, info);
            _tasks.Add(task);
            _tasks.RemoveAll(x => x.IsCompleted);

            await ToSignal(GetTree(), "process_frame");
        }

        Ps.AddInfo("Wait all tasks complete...");
        GD.Print("Wait all tasks complete...");
        Stop = true;
        Task.Run(() =>
        {
            GD.Print("Pause new Task...");
            Task.WaitAll(_tasks.ToArray());
            Stop = false;
        });
        while (Stop) await ToSignal(GetTree(), "process_frame");
        GD.Print("OK!");
        Ps.AddInfo("OK!");
    }

    public Task Process(Texture2D texture2D, string json, string outputPath, Info? info = null)
    {
        using var image         = texture2D.GetImage();
        var       animationData = JsonConvert.DeserializeObject<SpriteJsonData>(json);
        if (animationData is null) throw new Exception("Animation data could not be deserialized");

        var spriteFrames = GetSpriteFrames(animationData, image);

        return Task.Run(() =>
        {
            ResourceSaver.Save(spriteFrames, outputPath);
            spriteFrames?.Dispose();
            info?.CallDeferred("SetCheck", true);
        });
    }

    public SpriteFrames? GetSpriteFrames(SpriteJsonData data, Image image)
    {
        var sf = new SpriteFrames();
        foreach (var (animName, animData) in data.anims)
        {
            var name = animData.name.Replace(":", "_") + "_" + animName;
            if (sf.HasAnimation(name))
            {
                GD.PrintErr($"Animation {name} already exists");
                return null;
            }

            sf.AddAnimation(name);
            sf.SetAnimationSpeed(name, _animationSpeed);
            var rect = Rectangle.CalculateBound(animData.seq.Select(f => data.frames[f[0].ToString()]), data);
            foreach (var sequence in animData.seq)
            {
                using var emptyImage = GetEmptyImage(rect, image.GetFormat());
                var       frame      = data.frames[sequence[0].ToString()];
                using var cropImage  = GetFrameImage(frame, data, image);
                if (frame.parts.Count > 0)
                {
                    var frameRect = Rectangle.CalculateBound(frame, data);
                    var src       = new Rect2I(0, 0, frameRect.Size);
                    var dst       = frameRect.Position - rect.Position;
                    emptyImage.BlendRect(cropImage, src, dst);
                }

                if (sf.HasAnimation(name))
                    sf.AddFrame(name, ImageTexture.CreateFromImage(emptyImage));
                else
                    GD.PrintErr($"Animation {name} not found");
            }
        }

        return sf;
    }

    public static Image GetEmptyImage(Rectangle rect, Image.Format format)
    {
        return Image.CreateEmpty(rect.Size.X, rect.Size.Y, false, format);
    }

    public static Image GetFrameImage(FrameData frame, SpriteJsonData animationData, Image image)
    {
        var bound  = Rectangle.CalculateBound(frame, animationData);
        var region = GetEmptyImage(bound, image.GetFormat());
        foreach (var part in frame.parts)
        {
            var module = animationData.modules[part[0].ToString()];
            var dst = new Vector2I(Convert.ToInt32(part[1]) - bound.Position.X,
                Convert.ToInt32(part[2])                    - bound.Position.Y);
            var src = new Rect2I(
                new Vector2I((int)module.rect[0], (int)module.rect[1]),
                new Vector2I((int)module.rect[2], (int)module.rect[3]));
            region.BlendRect(image, src, dst);
        }

        return region;
    }
}