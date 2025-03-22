using System;
using System.Collections.Generic;
using System.Linq;
using EastwardRipping.AnimationJsonData;
using Godot;
using Newtonsoft.Json;

namespace EastwardRipping;

public partial class AnimationController : Node
{
    [Export] public Timer Timer;

    [Export] public Resource  Json;
    [Export] public Texture2D Atlas;

    [Export] public Texture2D CurrentFrame;

    [Export] public int   CurrentFrameIndex;
    [Export] public float PlaySpeed = 10 / 60f;

    public Image atlasImage { get; set; }

    public SpriteJsonData currentJsonData { get; set; }

    public AnimationData currentAnimationData { get; set; }

    public List<Texture2D> currentAnimationTextures { get; } = new();

    public List<string> currentFrameParts { get; } = new();

    public List<string> ignoreTables { get; } = new();

    public bool isAnimationFinished => CurrentFrameIndex >= currentAnimationTextures.Count - 1;

    public bool isLoop { get; set; }

    public float animationDuration => CurrentFrameIndex / PlaySpeed;

    public void Play(string animationName)
    {
        currentAnimationData = currentJsonData.anims[animationName];
        GetSpriteFrames();
        Timer.Start();
    }

    public void Init()
    {
        atlasImage = Atlas.GetImage();
        var json = FileAccess.Open(Json.GetPath(), FileAccess.ModeFlags.Read).GetAsText();
        currentJsonData = JsonConvert.DeserializeObject<SpriteJsonData>(json);
        Timer.Stop();
        Timer.Timeout += () => SetCurrentFrame(CurrentFrameIndex++);
    }

    public void SetCurrentFrame(int index)
    {
        if (currentAnimationTextures.Count == 0) return;
        if (index < 0 || index >= currentAnimationTextures.Count)
        {
            if (!isLoop) return;
            CurrentFrameIndex = 0;
            index             = 0;
        }

        CurrentFrame = currentAnimationTextures[index];
    }

    public void GetSpriteFrames()
    {
        currentAnimationTextures.Clear();
        currentFrameParts.Clear();
        var rect = Rectangle
           .CalculateBound(
                currentAnimationData.seq
                                    .Select(f => currentJsonData.frames[f[0].ToString()]), currentJsonData);
        foreach (var sequence in currentAnimationData.seq)
        {
            var       frameId    = sequence[0].ToString();
            using var emptyImage = GetClipFrameImage(rect, frameId);
            currentAnimationTextures.Add(ImageTexture.CreateFromImage(emptyImage));
            currentFrameParts.Add(sequence[0].ToString());
        }
    }

    public Image GetClipFrameImage(Rectangle rect, string frameId)
    {
        var       emptyImage = GetEmptyImage(rect, atlasImage.GetFormat());
        var       frame      = currentJsonData.frames[frameId];
        using var cropImage  = GetFrameImage(frame, currentJsonData, atlasImage);
        if (frame.parts.Count <= 0) return emptyImage;
        var frameRect = Rectangle.CalculateBound(frame, currentJsonData);
        var src       = new Rect2I(0, 0, frameRect.Size);
        var dst       = frameRect.Position - rect.Position;
        emptyImage.BlendRect(cropImage, src, dst);
        return emptyImage;
    }

    public Image GetEmptyImage(Rectangle rect, Image.Format format)
    {
        return Image.CreateEmpty(rect.Size.X, rect.Size.Y, false, format);
    }

    public Image GetFrameImage(FrameData frame, SpriteJsonData animationData, Image image)
    {
        var bound  = Rectangle.CalculateBound(frame, animationData);
        var region = GetEmptyImage(bound, image.GetFormat());
        foreach (var part in frame.parts)
        {
            if (ignoreTables.Contains(part[0].ToString())) continue;
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