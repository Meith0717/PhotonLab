// CpuTexture.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhotonLab.Source.RayTracing
{
    internal class CpuTexture2D
    {
        public readonly Texture2D Texture2D;
        public readonly int Width;
        public readonly int Height;
        private readonly Color[] _colorData;

        public CpuTexture2D(Texture2D texture2D)
        {
            Texture2D = texture2D;
            Width = texture2D.Width;
            Height = texture2D.Height;
            _colorData = new Color[Width * Height];

            texture2D.GetData(_colorData);
        }

        public Color SampleData(Vector2 uv)
        {
            uv = Vector2.Clamp(uv, new(0), new(1));

            var x = (int)(uv.X * (Width - 1));
            var y = (int)(uv.Y * (Height - 1));

            return SampleData(x, y);
        }

        public Color SampleData(Point point) 
            => SampleData(point.X, point.Y);

        public Color SampleData(int x, int y)
            => _colorData[y * Width + x];
    }
}
