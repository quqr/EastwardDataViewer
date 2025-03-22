#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EastwardRipping.AnimationJsonData;
using Godot;
using Newtonsoft.Json;
using FileAccess = Godot.FileAccess;

namespace EastwardRipping.Scripts;

[Tool]
public partial class Viewer : Control
{
    [Export] private Control _dataSourceContainer;
    [Export] private Control _framesContainer;
    [Export] private Control _partContainer;
    [Export] private Control _animationSourceContainer;

    [Export] private TextureRect _currentFrame;

    [Export] private Label _frameInfoLabel;

    [Export] private Button _nextButton;
    [Export] private Button _playButton;
    [Export] private Button _prevButton;
    [Export] private Button _resetButton;
    [Export] private Button _pauseButton;

    [Export] private Timer _timer;

    [Export] private Resource  _json;
    [Export] private Texture2D _atlas;

    [Export] private int   _currentFrameIndex;
    [Export] private float _playSpeed = 10 / 60f;

    private Image atlasImage { get; set; }

    private SpriteJsonData currentJsonData { get; set; }

    private AnimationData currentAnimationData { get; set; }

    private List<Texture2D> textures { get; } = new();

    private List<string> parts { get; } = new();

    private Dictionary<string, bool> ignoreTables { get; } = new();

    public override void _Ready()
    {
        _currentFrame.Texture = null;
        _framesContainer.RemoveAllChildren();
        _animationSourceContainer.RemoveAllChildren();
        _dataSourceContainer.RemoveAllChildren();


        _timer.WaitTime        =  _playSpeed;
        _timer.Timeout         += () => SetCurrentFrame(++_currentFrameIndex);
        _nextButton.ButtonDown += () => SetCurrentFrame(++_currentFrameIndex);
        _prevButton.ButtonDown += () => SetCurrentFrame(--_currentFrameIndex);
        _resetButton.ButtonDown += () =>
        {
            SetCurrentFrame(0);
            _timer.Stop();
        };
        _pauseButton.ButtonDown += () => { _timer.Stop(); };
        _playButton.ButtonDown += () => { _timer.Start(); };
        LoadResources();
    }

    public void LoadResources()
    {
        var dirs = Directory.GetDirectories("anim", "*", SearchOption.AllDirectories);

        foreach (var dir in dirs)
        {
            var files     = Directory.GetFiles(dir);
            var atlasPath = Path.Combine(dir, "atlas.png");
            var jsonPath  = Path.Combine(dir, "def.json");
            if (!FileAccess.FileExists(atlasPath) || !FileAccess.FileExists(jsonPath))
            {
                var atlas = files.Where(x => x.EndsWith("texture")).ToArray();
                if (atlas.Length == 0) continue;
                File.Move(atlas[0], $"{atlas[0]}.png");
                atlasPath = $"{atlas[0]}.png";
            }

            var button = new Button();
            button.Text = Path.GetFileName(dir.Split(".")[0]);
            button.ButtonDown += () =>
            {
                _atlas = ResourceLoader.Load<Texture2D>(atlasPath);
                _json  = ResourceLoader.Load<Resource>(jsonPath);
                Process();
            };
            _dataSourceContainer.AddChild(button);
        }
    }

    public void Process()
    {
        atlasImage = _atlas.GetImage();
        var json = FileAccess.Open(_json.GetPath(), FileAccess.ModeFlags.Read).GetAsText();
        currentJsonData = JsonConvert.DeserializeObject<SpriteJsonData>(json);

        _partContainer.RemoveAllChildren();
        _animationSourceContainer.RemoveAllChildren();
        _framesContainer.RemoveAllChildren();
        _currentFrame.Texture = null;
        _frameInfoLabel.Text  = string.Empty;
        _timer.Stop();

        SetAnimationsButton(currentJsonData);
    }

    public void SetAnimationsButton(SpriteJsonData data)
    {
        foreach (var (_, animData) in data.anims)
        {
            var button = new Button();
            button.Text = animData.name;
            button.ButtonDown += () =>
            {
                currentAnimationData = animData;
                SetAnimationTexture();
                SetCurrentFrame(0);
            };
            _animationSourceContainer.AddChild(button);
        }
    }

    public void SetAnimationTexture()
    {
        _framesContainer.RemoveAllChildren();
        GetSpriteFrames();
        foreach (var frame in textures)
        {
            var texture = new TextureRect();
            texture.TextureFilter = TextureFilterEnum.Nearest;
            texture.Texture       = frame;
            texture.StretchMode   = TextureRect.StretchModeEnum.KeepAspectCentered;
            _framesContainer.AddChild(texture);
        }
    }

    public void SetCurrentFrame(int index)
    {
        if (textures.Count == 0) return;
        if (index < 0 || index >= textures.Count)
        {
            _currentFrameIndex = 0;
            index              = 0;
        }

        LoadParts(index);
        _frameInfoLabel.Text  = $"{index + 1}/{textures.Count}";
        _currentFrame.Texture = textures[index];
    }

    public void LoadParts(int index)
    {
        ignoreTables.Clear();
        _partContainer.RemoveAllChildren();
        var frame = currentJsonData.frames[parts[index]];
        foreach (var part in frame.parts)
        {
            ignoreTables.Add(part[0].ToString(), false);
            AddPartToContainer(part, index);
        }
    }

    private void AddPartToContainer(List<object> part, int index)
    {
        var button = new CheckButton();
        button.Text = part[0].ToString();
        button.SetPressed(true);
        button.ButtonDown += () =>
        {
            var id = part[0].ToString();
            ignoreTables[id] = !ignoreTables[id];
            var rect = Rectangle
               .CalculateBound(currentAnimationData.seq
                                                   .Select(f =>
                                                        currentJsonData.frames[f[0].ToString()]), currentJsonData);
            var img = GetClipFrameImage(rect, parts[index]);
            _currentFrame.Texture = ImageTexture.CreateFromImage(img);
        };
        _partContainer.AddChild(button);
    }

    public void GetSpriteFrames()
    {
        textures.Clear();
        parts.Clear();
        var rect = Rectangle
           .CalculateBound(
                currentAnimationData.seq
                                    .Select(f => currentJsonData.frames[f[0].ToString()]), currentJsonData);
        foreach (var sequence in currentAnimationData.seq)
        {
            var       frameId    = sequence[0].ToString();
            using var emptyImage = GetClipFrameImage(rect, frameId);
            textures.Add(ImageTexture.CreateFromImage(emptyImage));
            parts.Add(sequence[0].ToString());
        }
    }

    private Image GetClipFrameImage(Rectangle rect, string frameId)
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
            ignoreTables.TryGetValue(part[0].ToString(), out var isIgnore);
            if (isIgnore) continue;
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
#endif