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

        public CpuTexture2D Texture { get; }
        public Color Color { get; } = Color.White;
        public NormalMode NormalMode { get; }

        public Color AmbientColor = Color.White;

        public float DiffuseStrength = 1;
        public float SpecularStrength = 1;
        public float AmbientStrength = 1;
        public float SpecExponent = 40;

        public PhongMaterial(Texture2D albedo, NormalMode normalMode = NormalMode.Interpolated)
        {
            Texture = new CpuTexture2D(albedo);
            NormalMode = normalMode;
        }

        public PhongMaterial(Color diffuseColor, NormalMode normalMode = NormalMode.Interpolated)
        {
            Color = diffuseColor;
            NormalMode = normalMode;
        }

        public Radiance Shade(
            Scene scene,
            int depth,
            in RaySimd ray,
            in SurfaceIntersectionData surfaceData
        )
        {
            var normal = surfaceData.Normal;
            var hitPosition = surfaceData.Position;
            var v = Vector3.Normalize(scene.Camera3D.Position.ToNumerics() - hitPosition);

            var surfaceColor = Texture?.SampleData(surfaceData.TexturePos) ?? Color;
            var radiance = Radiance.Zero;
            radiance.Attenuate(AmbientColor, OneOverPi * AmbientStrength);

            radiance += scene.LightSources.Forall(
                in surfaceData,
                (lightRadiance, lightPosition) =>
                {
                    var lightDirection = lightPosition - hitPosition;
                    var distanceToLight = lightDirection.Length();
                    lightDirection = Vector3.Normalize(lightDirection);

                    var shadowRay = new RaySimd(hitPosition, lightDirection);
                    if (
                        scene.Meshes.Intersect(shadowRay, out var distance, out _)
                        && distance < distanceToLight
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
