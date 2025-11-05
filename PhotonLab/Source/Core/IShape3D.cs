// IShape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Core;

namespace PhotonLab.Source.Core
{
    internal interface IShape3D
    {
        public Matrix ModelTransform { get; }

        public Material Material { get; }

        public bool Intersect(Ray ray, out HitInfo hit);

        public void Draw(GraphicsDevice graphics, BasicEffect effect);
    }
}
