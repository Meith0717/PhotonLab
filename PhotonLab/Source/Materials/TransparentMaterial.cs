// TransparentMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Materials
{
    internal class TransparentMaterial : IMaterial
    {
        private readonly NormalMode _normalMode;

        public CpuTexture2D DiffuseTexture { get; } = null!;
        public Color DiffuseColor { get; } = Color.White;
        public float RefractiveIndex { get; } = 1.2f;
        public float ReflectetStrength { get; } = 1f;

        public TransparentMaterial(NormalMode normalMode = NormalMode.Interpolated)
        {
            _normalMode = normalMode;
        }

        public TransparentMaterial(
            Microsoft.Xna.Framework.Color tint,
            NormalMode normalMode = NormalMode.Interpolated
        )
        {
            DiffuseColor = tint;
            _normalMode = normalMode;
        }

        public Radiance Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
        {
            var n = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException(),
            };

            var hitPosition = hit.Position;

            // Compute reflection direction
            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var reflectedRay = new RaySIMD(hitPosition, reflectDir);
            var reflectedRadiance = RayTracer.Trace(scene, reflectedRay, depth + 1);
            reflectedRadiance.Attenuate(ReflectetStrength);

            // Determine if we’re entering or exiting the medium
            var cosi = float.Clamp(Vector3.Dot(ray.Direction, n), -1, 1);
            var etai = 1.0f;
            var etat = RefractiveIndex;

            if (cosi > 0)
                (etai, etat, n) = (etat, etai, -n);

            var eta = etai / etat;
            var k = 1 - eta * eta * (1 - cosi * cosi);

            var refractedRadiance = Radiance.Zero;
            if (k >= 0)
            {
                var refractDir = Vector3.Normalize(
                    eta * ray.Direction - (eta * cosi + MathF.Sqrt(k)) * n
                );
                hitPosition -= n * (2 * RayTracingGlobal.HitOffsetEpsilon);
                var refractedRay = new RaySIMD(hitPosition, refractDir);
                refractedRadiance = RayTracer.Trace(scene, refractedRay, depth + 1);
            }

            // Fresnel reflectance (Schlick’s approximation)
            var R0 = MathF.Pow((etai - etat) / (etai + etat), 2);
            var fresnel = R0 + (1 - R0) * MathF.Pow(1 - MathF.Abs(cosi), 5);

            // Combine reflection + refraction
            var totalRadiance =
                reflectedRadiance.Attenuate(fresnel) + refractedRadiance.Attenuate(1 - fresnel);
            totalRadiance.Attenuate(DiffuseColor, 1);
            return totalRadiance;
        }
    }
}
