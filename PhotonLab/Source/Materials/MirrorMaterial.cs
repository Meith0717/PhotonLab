// MirrorMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Materials
{
    internal class MirrorMaterial : IMaterial
    {
        public CpuTexture2D Texture { get; } = null!;
        public Color Color { get; }
        public NormalMode NormalMode { get; }

        public Radiance Shade(
            Scene scene,
            int depth,
            in RaySIMD ray,
            in SurfaceIntersectionData surfaceData
        )
        {
            var n = surfaceData.Normal;

            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var reflectedRay = new RaySIMD(surfaceData.Position, reflectDir);
            var reflectedRadiance = RayTracer.Trace(scene, reflectedRay, depth + 1);

            return reflectedRadiance.Attenuate(Color, 1);
        }
    }
}
