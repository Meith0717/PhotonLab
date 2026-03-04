// SurfaceIntersectionData.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Numerics;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.RayTracing
{
    internal readonly struct SurfaceIntersectionData(
        Vector3 position,
        float distance,
        Vector3 normal,
        Vector2 texturePos,
        IMaterial material
    )
    {
        public Vector3 Position { get; } = position;
        public float Distance { get; } = distance;
        public Vector3 Normal { get; } = normal;
        public Vector2 TexturePos { get; } = texturePos;
        public IMaterial Material { get; } = material;

        public SurfaceIntersectionData()
            : this(Vector3.Zero, float.MaxValue, Vector3.Zero, Vector2.Zero, null)
        {
            ;
        }

        public static bool operator <(SurfaceIntersectionData a, SurfaceIntersectionData b) =>
            a.Distance < b.Distance;

        public static bool operator >(SurfaceIntersectionData a, SurfaceIntersectionData b) =>
            a.Distance > b.Distance;

        public static bool operator <=(SurfaceIntersectionData a, SurfaceIntersectionData b) =>
            a.Distance <= b.Distance;

        public static bool operator >=(SurfaceIntersectionData a, SurfaceIntersectionData b) =>
            a.Distance >= b.Distance;
    }
}
