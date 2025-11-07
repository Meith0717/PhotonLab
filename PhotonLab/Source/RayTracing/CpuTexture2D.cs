// CpuTexture.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PhotonLab.Source.RayTracing
{
    internal class CpuTexture2D
    {
        public readonly Texture2D Texture2D;
        public readonly int Width;
        public readonly int Height;
        private readonly System.Numerics.Vector4[] _colorData;

        public CpuTexture2D(Texture2D texture2D)
        {
            Texture2D = texture2D;
            Width = texture2D.Width;
            Height = texture2D.Height;

            var colorData = new Color[Width * Height];
            texture2D.GetData(colorData);

            _colorData = Array.ConvertAll(colorData, c => c.ToVector4().ToNumerics());
        }

        public System.Numerics.Vector3 SampleData3(Vector2 uv)
        {
            uv = Vector2.Clamp(uv, new(0), new(1));

            var x = (int)(uv.X * (Width - 1));
            var y = (int)(uv.Y * (Height - 1));

            var data = SampleData(x, y);
            return new(data.X, data.Y, data.Z);
        }

        public System.Numerics.Vector4 SampleData4(Vector2 uv)
        {
            uv = Vector2.Clamp(uv, new(0), new(1));

            var x = (int)(uv.X * (Width - 1));
            var y = (int)(uv.Y * (Height - 1));

            return SampleData(x, y);
        }

        public System.Numerics.Vector4 SampleData(Point point) 
            => SampleData(point.X, point.Y);

        public System.Numerics.Vector4 SampleData(int x, int y)
            => _colorData[y * Width + x];
    }
}
