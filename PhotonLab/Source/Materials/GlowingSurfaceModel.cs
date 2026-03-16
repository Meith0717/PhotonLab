// GlowingSurfaceModel.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.RayTracing.Data;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Materials;

internal class GlowingSurfaceModel(NormalMode normalMode, Color color, float intensity)
    : ISurfaceModel,
        IColoredSurface
{
    public NormalMode NormalMode { get; } = normalMode;
    public Color Color { get; } = color;

    public Radiance Shade(
        Scene scene,
        int depth,
        in RaySimd ray,
        in SurfaceIntersectionData surfaceData
    )
    {
        return new Radiance(Color).Attenuate(intensity);
    }
}
