// TransparentMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using System;
using System.Numerics;

namespace PhotonLab.Source.Materials
{
    internal class TransparentMaterial : IMaterial
    {
        private readonly NormalMode _normalMode;

        public CpuTexture2D DiffuseTexture { get; } = null!;
        public Vector3 DiffuseColor { get; } = Vector3.One;
        public float RefractiveIndex { get; } = 1.2f;
        public float ReflectetStrength { get; } = 1f;

        public TransparentMaterial(NormalMode normalMode = NormalMode.Interpolated)
        {
            _normalMode = normalMode;
        }

        public TransparentMaterial(Microsoft.Xna.Framework.Color tint, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseColor = tint.ToVector3().ToNumerics();
            _normalMode = normalMode;
        }

        public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit, out byte hitCount)
        {
            var n = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException()
            };

            var hitPosition = ray.Position + ray.Direction * hit.Distance;

            // Compute reflection direction
            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var reflectedRay = new RaySIMD(hitPosition, reflectDir);
            var reflectedColor = ReflectetStrength * RayTracer.Trace(scene, reflectedRay, depth + 1, out hitCount);

            // Determine if we’re entering or exiting the medium
            var cosi = float.Clamp(Vector3.Dot(ray.Direction, n), -1, 1);
            var etai = 1.0f;
            var etat = RefractiveIndex;

            if (cosi > 0)
                (etai, etat, n) = (etat, etai, -n);

            var eta = etai / etat;
            var k = 1 - eta * eta * (1 - cosi * cosi);

            var refractedColor = Vector3.Zero;
            if (k >= 0)
            {
                var refractDir = Vector3.Normalize(eta * ray.Direction - (eta * cosi + MathF.Sqrt(k)) * n);
                var refractedRay = new RaySIMD(hitPosition, refractDir);
                refractedColor = RayTracer.Trace(scene, refractedRay, depth + 1, out var hits);
                hitCount += hits;
            }

            // Fresnel reflectance (Schlick’s approximation)
            var R0 = MathF.Pow((etai - etat) / (etai + etat), 2);
            var fresnel = R0 + (1 - R0) * MathF.Pow(1 - MathF.Abs(cosi), 5);

            // Combine reflection + refraction
            var color = fresnel * reflectedColor + (1 - fresnel) * refractedColor;
            return DiffuseColor * color;
        }
    }
}
