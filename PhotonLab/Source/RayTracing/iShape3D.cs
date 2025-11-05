// IShape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Core
{
    internal interface IShape3D
    {
        public Matrix ModelTransform { get; }

        public IMaterial Material { get; }

        public bool Intersect(Ray ray, out HitInfo hit);

        public void Draw(GraphicsDevice graphics, BasicEffect effect);
    }
}
