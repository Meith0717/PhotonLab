// PhongMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Materials
{
    internal class PhongMaterial : IMaterial
    {
        private const float OneOverPi = 1; // 1 / float.Pi;
        private readonly NormalMode _normalMode;

        public CpuTexture2D DiffuseTexture { get; }

        public Color DiffuseColor { get; } = Color.White;
        public Color AmbientColor = Color.White;

        public float DiffuseStrength = 1;
        public float SpecularStrength = 1;
        public float AmbientStrength = 1;
        public float SpecExponent = 40;

        public PhongMaterial(Texture2D albedo, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseTexture = new CpuTexture2D(albedo);
            _normalMode = normalMode;
        }

        public PhongMaterial(Color diffuseColor, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseColor = diffuseColor;
            _normalMode = normalMode;
        }

        public Radiance Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
        {
            var normal = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException(),
            };
            var hitPosition = hit.Position;
            var v = Vector3.Normalize(scene.Camera3D.Position.ToNumerics() - hitPosition);

            var surfaceColor = DiffuseTexture?.SampleData(hit.TexturePos) ?? DiffuseColor;
            var radiance = Radiance.Zero;
            radiance.Attenuate(AmbientColor, OneOverPi * AmbientStrength);

            radiance += scene.LightSources.Forall(
                in hit,
                (lightRadiance, lightPosition) =>
                {
                    var lightDirection = lightPosition - hitPosition;
                    var distanceToLight = lightDirection.Length();
                    lightDirection = Vector3.Normalize(lightDirection);

                    var shadowRay = new RaySIMD(hitPosition, lightDirection);
                    if (
                        scene.Meshes.Intersect(shadowRay, out var shadowHit)
                        && shadowHit.Distance < distanceToLight
                    )
                        return Radiance.Zero;

                    var nDotL = MathF.Max(Vector3.Dot(normal, lightDirection), 0);
                    var diffuseRadiance = lightRadiance.Attenuate(surfaceColor, OneOverPi * nDotL);

                    var r = Vector3.Reflect(-lightDirection, normal);
                    var rDotV = MathF.Pow(MathF.Max(Vector3.Dot(r, v), 0), SpecExponent);
                    var specularRadiance = lightRadiance.Attenuate(surfaceColor, rDotV);

                    return diffuseRadiance.Attenuate(DiffuseStrength)
                        + specularRadiance.Attenuate(SpecularStrength);
                }
            );

            return radiance;
        }
    }
}
