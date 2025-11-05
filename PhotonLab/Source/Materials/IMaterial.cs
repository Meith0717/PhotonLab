// IMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.Core;
using PhotonLab.Source.RayTracing;

namespace PhotonLab.Source.Materials
{
    internal interface IMaterial
    {
        public CpuTexture2D Albedo { get; }

        public Color Shade(Scene scene, int depth, Ray ray, in HitInfo hit);
    }
}
