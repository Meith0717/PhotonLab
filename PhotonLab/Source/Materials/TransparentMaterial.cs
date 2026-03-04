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
        public CpuTexture2D Texture { get; } = null!;
        public Color Color { get; } = Color.White;
        public NormalMode NormalMode { get; }
        public float RefractiveIndex { get; } = 1.2f;
        public float ReflectetStrength { get; } = 1f;

        public TransparentMaterial(NormalMode normalMode = NormalMode.Interpolated)
        {
            NormalMode = normalMode;
        }

        public TransparentMaterial(
            Microsoft.Xna.Framework.Color tint,
            NormalMode normalMode = NormalMode.Interpolated
        )
        {
            Color = tint;
            NormalMode = normalMode;
        }

        public Radiance Shade(
            Scene scene,
            int depth,
            in RaySimd ray,
            in SurfaceIntersectionData surfaceData
        )
        {
            var n = surfaceData.Normal;

            var hitPosition = surfaceData.Position;

            // Compute reflection direction
            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var reflectedRay = new RaySimd(hitPosition, reflectDir);
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
                var refractedRay = new RaySimd(hitPosition, refractDir);
                refractedRadiance = RayTracer.Trace(scene, refractedRay, depth + 1);
            }

            // Fresnel reflectance (Schlick’s approximation)
            var R0 = MathF.Pow((etai - etat) / (etai + etat), 2);
            var fresnel = R0 + (1 - R0) * MathF.Pow(1 - MathF.Abs(cosi), 5);

            // Combine reflection + refraction
            var totalRadiance =
                reflectedRadiance.Attenuate(fresnel) + refractedRadiance.Attenuate(1 - fresnel);
            totalRadiance.Attenuate(Color, 1);
            return totalRadiance;
        }
    }
}
