// SurfaceIntersectionData.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Numerics;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.RayTracing
{
    internal readonly struct SurfaceIntersectionData(
        Vector3 position,
        Vector3 normal,
        Vector2 texturePos,
        ISurfaceModel surfaceModel
    )
    {
        public Vector3 Position { get; } = position;
        public Vector3 Normal { get; } = normal;
        public Vector2 TexturePos { get; } = texturePos;
        public ISurfaceModel SurfaceModel { get; } = surfaceModel;
    }
}
