// GlowingMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Materials;

internal class GlowingMaterial : IMaterial
{
    public CpuTexture2D Texture { get; set; }
    public NormalMode NormalMode { get; set; }
    public Color Color { get; set; }

    public Radiance Shade(
        Scene scene,
        int depth,
        in RaySimd ray,
        in SurfaceIntersectionData surfaceData
    )
    {
        var color = Texture?.SampleData(surfaceData.TexturePos) ?? Color;
        return new Radiance(color);
    }
}
