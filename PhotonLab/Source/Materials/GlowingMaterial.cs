// GlowingMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Numerics;
using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Materials;

internal class GlowingMaterial : IMaterial
{
    public CpuTexture2D DiffuseTexture { get; set; }

    public Color DiffuseColor { get; set; }

    public Radiance Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
    {
        var color = DiffuseTexture?.SampleData(hit.TexturePos) ?? DiffuseColor;
        return new Radiance(color);
    }
}
