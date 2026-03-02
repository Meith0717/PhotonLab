// GlowingMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Numerics;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Materials;

internal class GlowingMaterial : IMaterial
{
    public CpuTexture2D DiffuseTexture { get; set; }

    public Vector3 DiffuseColor { get; set; }

    public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
    {
        return DiffuseTexture?.SampleData3(hit.TexturePos) ?? DiffuseColor;
    }
}
