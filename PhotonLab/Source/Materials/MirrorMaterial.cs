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
    internal class MirrorMaterial(
        Color color,
        float reflectivity = 1f,
        NormalMode normalMode = NormalMode.Interpolated
    ) : IMaterial
    {
        public CpuTexture2D DiffuseTexture { get; } = null!;
        public Color DiffuseColor { get; } = color;

        public Radiance Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
        {
            var n = normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException(),
            };

            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var reflectedRay = new RaySIMD(hit.Position, reflectDir);
            var reflectedRadiance = RayTracer.Trace(scene, reflectedRay, depth + 1);

            return reflectedRadiance.Attenuate(DiffuseColor, Math.Clamp(reflectivity, 0f, 1f));
        }
    }
}
