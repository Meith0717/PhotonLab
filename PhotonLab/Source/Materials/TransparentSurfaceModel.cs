// TransparentSurfaceModel.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using Microsoft.Xna.Framework;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.RayTracing.Data;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Materials
{
    internal class TransparentSurfaceModel(NormalMode normalMode, float refractiveIndex)
        : ISurfaceModel
    {
        public NormalMode NormalMode { get; } = normalMode;

        public Radiance Shade(
            Scene scene,
            int depth,
            in RaySimd ray,
            in SurfaceIntersectionData surfaceData
        )
        {
            var normal = surfaceData.Normal;
            var hitPosition = surfaceData.Position;

            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, normal));
            var reflectedRay = new RaySimd(hitPosition, reflectDir);
            var reflectedRadiance = RayTracer.Trace(scene, reflectedRay, depth + 1);

            // Determine if we’re entering or exiting the medium
            var cosi = float.Clamp(Vector3.Dot(ray.Direction, normal), -1, 1);
            var etai = 1.0f;
            var etat = refractiveIndex;

            if (cosi > 0)
                (etai, etat, normal) = (etat, etai, -normal);

            var eta = etai / etat;
            var k = 1 - eta * eta * (1 - cosi * cosi);

            var refractedRadiance = Radiance.Zero;
            if (k >= 0)
            {
                var refractDir = Vector3.Normalize(
                    eta * ray.Direction - (eta * cosi + MathF.Sqrt(k)) * normal
                );
                hitPosition -= normal * (2 * RayTracingGlobal.HitOffsetEpsilon);
                var refractedRay = new RaySimd(hitPosition, refractDir);
                refractedRadiance = RayTracer.Trace(scene, refractedRay, depth + 1);
            }

            // Fresnel reflectance (Schlick’s approximation)
            var R0 = MathF.Pow((etai - etat) / (etai + etat), 2);
            var fresnel = R0 + (1 - R0) * MathF.Pow(1 - MathF.Abs(cosi), 5);

            // Combine reflection + refraction
            var totalRadiance =
                reflectedRadiance.Attenuate(fresnel) + refractedRadiance.Attenuate(1 - fresnel);
            return totalRadiance;
        }
    }
}
