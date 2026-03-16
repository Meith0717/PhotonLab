// CpuTexture2D.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhotonLab.Source.RayTracing.Data
{
    internal class CpuTexture2D
    {
        public readonly Texture2D Texture2D;
        private readonly int _width;
        private readonly int _height;
        private readonly Color[] _colorData;

        public CpuTexture2D(Texture2D texture2D)
        {
            Texture2D = texture2D;
            _width = texture2D.Width;
            _height = texture2D.Height;
            _colorData = new Color[_width * _height];
            texture2D.GetData(_colorData);
        }

        public Color SampleData(Vector2 uv)
        {
            uv = Vector2.Clamp(uv, new Vector2(0), new Vector2(1));
            var x = (int)(uv.X * (_width - 1));
            var y = (int)(uv.Y * (_height - 1));
            return SampleData(x, y);
        }

        private Color SampleData(int x, int y) => _colorData[y * _width + x];
    }
}
