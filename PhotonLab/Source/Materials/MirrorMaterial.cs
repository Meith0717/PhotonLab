// MirrorMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using System;
using System.Numerics;

namespace PhotonLab.Source.Materials
{
    internal class MirrorMaterial(Microsoft.Xna.Framework.Color color, float reflectivity = 1f, NormalMode normalMode = NormalMode.Interpolated) : IMaterial
    {
        public CpuTexture2D DiffuseTexture { get; } = null!;
        public Vector3 DiffuseColor { get; } = color.ToVector3().ToNumerics();
        private readonly NormalMode _normalMode = normalMode;
        public float Reflectivity { get; } = Math.Clamp(reflectivity, 0f, 1f);

        public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit, out byte hitCount)
        {
            var n = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException()
            };
            
            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var hitPosition = ray.Position + ray.Direction * hit.Distance;
            var reflectedRay = new RaySIMD(hitPosition, reflectDir);
            var reflectedColor = RayTracer.Trace(scene, reflectedRay, depth + 1, out hitCount);

            return DiffuseColor * Reflectivity * reflectedColor;
        }
    }
}
