// IMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Materials
{
    public enum NormalMode
    {
        Face,
        Interpolated,
    }

    internal interface ISurface { }

    internal interface ISurfaceModel : ISurface
    {
        NormalMode NormalMode { get; }

        Radiance Shade(
            Scene scene,
            int depth,
            in RaySimd ray,
            in SurfaceIntersectionData surfaceData
        );
    }

    internal interface IColoredSurface : ISurface
    {
        Color Color { get; }
    }

    internal interface ITexturedSurface : ISurface
    {
        CpuTexture2D Texture { get; }
    }
}
