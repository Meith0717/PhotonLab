// Material.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace PhotonLab.Source.Core
{
    internal abstract class Material : IMaterial
    {
        public readonly Texture2D Texture;
        private readonly Color[] _textureData;
        private readonly Size _size;

        public Material(Texture2D texture)
        {
            Texture = texture;
            _size = new(texture.Width, texture.Height);
            _textureData = new Color[_size.Width * _size.Height];
            texture.GetData(_textureData);
        }

        public Color SampleTexture(Vector2 uv)
        {
            uv.X = float.Clamp(uv.X, 0f, 1f);
            uv.Y = float.Clamp(uv.Y, 0f, 1f);

            int x = (int)(uv.X * (_size.Width - 1));
            int y = (int)(uv.Y * (_size.Height - 1));

            return _textureData[y * _size.Width + x];
        }

        public abstract Color Shade(Scene scene, int depth, Ray ray, in HitInfo hit);
    }
}
