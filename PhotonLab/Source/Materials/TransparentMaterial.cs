// TransparentMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Numerics;
using PhotonLab.Source.RayTracing;

namespace PhotonLab.Source.Materials
{
    internal class TransparentMaterial : IMaterial
    {
        private readonly NormalMode _normalMode;

        public CpuTexture2D DiffuseTexture { get; } = null!;
        public Vector3 DiffuseColor { get; } = Vector3.One;
        public float RefractiveIndex { get; } = 1.3f;

        public TransparentMaterial(NormalMode normalMode = NormalMode.Interpolated)
        {
            _normalMode = normalMode;
        }

        public TransparentMaterial(Microsoft.Xna.Framework.Color tint, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseColor = tint.ToVector3().ToNumerics();
            _normalMode = normalMode;
        }

        public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
        {
            var n = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException()
            };

            var hitPosition = ray.Position + ray.Direction * hit.Distance;
            hitPosition += n * IMaterial.Epsilon;

            // Compute reflection direction
            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, n));
            var reflectedRay = new RaySIMD(hitPosition, reflectDir);
            var reflectedColor = RayTracer.Trace(scene, reflectedRay, depth + 1);

            // Determine if we’re entering or exiting the medium
            float cosi = float.Clamp(Vector3.Dot(ray.Direction, n), -1, 1);
            float etai = 1.0f;
            float etat = RefractiveIndex;

            if (cosi > 0)
                (etai, etat, n) = (etat, etai, -n);

            float eta = etai / etat;
            float k = 1 - eta * eta * (1 - cosi * cosi);

            Vector3 refractedColor = Vector3.Zero;
            if (k >= 0)
            {
                var refractDir = Vector3.Normalize(eta * ray.Direction - (eta * cosi + MathF.Sqrt(k)) * n);
                hitPosition -= n * (2 * IMaterial.Epsilon);
                var refractedRay = new RaySIMD(hitPosition, refractDir);
                refractedColor = RayTracer.Trace(scene, refractedRay, depth + 1);
            }

            // Fresnel reflectance (Schlick’s approximation)
            float R0 = MathF.Pow((etai - etat) / (etai + etat), 2);
            float fresnel = R0 + (1 - R0) * MathF.Pow(1 - MathF.Abs(cosi), 5);

            // Combine reflection + refraction
            var color = fresnel * reflectedColor + (1 - fresnel) * refractedColor;
            return DiffuseColor * color;
        }
    }
}
