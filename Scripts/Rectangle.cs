using System;
using System.Collections.Generic;
using EastwardRipping.AnimationJsonData;
using Godot;

namespace EastwardRipping;

public struct Rectangle
{
    public Vector2I Position;
    public Vector2I Size;

    public Rectangle(Vector2I position, Vector2I size)
    {
        Position = position;
        Size     = size;
    }

    public override string ToString()
    {
        return $"Position: {Position}, Size: {Size}";
    }

    public static Rectangle CalculateBound(FrameData frame, SpriteJsonData animationData)
    {
        int xMin = int.MaxValue, yMin = int.MaxValue, xMax = int.MinValue, yMax = int.MinValue;
        foreach (var part in frame.parts)
        {
            var module = animationData.modules[part[0].ToString()];
            var min    = new Vector2I(Convert.ToInt32(part[1]), Convert.ToInt32(part[2]));
            var max    = min + new Vector2I((int)module.rect[2], (int)module.rect[3]);

            if (xMin > min.X) xMin = min.X;

            if (yMin > min.Y) yMin = min.Y;

            if (xMax < max.X) xMax = max.X;

            if (yMax < max.Y) yMax = max.Y;
        }

        return new Rectangle(new Vector2I(xMin, yMin), new Vector2I(xMax - xMin, yMax - yMin));
    }

    public static Rectangle CalculateBound(IEnumerable<FrameData> frames, SpriteJsonData animationData)
    {
        int xMin = int.MaxValue, yMin = int.MaxValue, xMax = int.MinValue, yMax = int.MinValue;
        foreach (var frame in frames)
        {
            foreach (var part in frame.parts)
            {
                var module = animationData.modules[part[0].ToString()];
                var min    = new Vector2I(Convert.ToInt32(part[1]), Convert.ToInt32(part[2]));
                var max    = min + new Vector2I((int)module.rect[2], (int)module.rect[3]);

                if (xMin > min.X) xMin = min.X;

                if (yMin > min.Y) yMin = min.Y;

                if (xMax < max.X) xMax = max.X;

                if (yMax < max.Y) yMax = max.Y;
            }
        }

        return new Rectangle(new Vector2I(xMin, yMin), new Vector2I(xMax - xMin, yMax - yMin));
    }
}