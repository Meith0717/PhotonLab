// PhongMaterial.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Materials
{
    internal class PhongMaterial : IMaterial
    {
        private const float OneOverPi = 1; // 1 / float.Pi;
        private readonly NormalMode _normalMode;

        public CpuTexture2D DiffuseTexture { get; }

        public Vector3 DiffuseColor { get; } = Vector3.One;
        public Vector3 AmbientColor = Vector3.One;

        public float DiffuseStrength = 1;
        public float SpecularStrength = 1;
        public float AmbientStrength = 1;
        public float SpecExponent = 40;

        public PhongMaterial(Texture2D albedo, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseTexture = new CpuTexture2D(albedo);
            _normalMode = normalMode;
        }

        public PhongMaterial(
            Microsoft.Xna.Framework.Color diffuseColor,
            NormalMode normalMode = NormalMode.Interpolated
        )
        {
            DiffuseColor = diffuseColor.ToVector3().ToNumerics();
            _normalMode = normalMode;
        }

        public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
        {
            var surfaceColor = DiffuseTexture?.SampleData3(hit.TexturePos) ?? DiffuseColor;
            var color = OneOverPi * AmbientStrength * AmbientColor * surfaceColor;
            var normal = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException(),
            };

            var hitPosition = hit.Position;
            var v = Vector3.Normalize(scene.Camera3D.Position.ToNumerics() - hitPosition);

            color += scene.LightSources.Forall(
                in hit,
                (lightColor, lightPosition) =>
                {
                    var lightDirection = lightPosition - hitPosition;
                    var distanceToLight = lightDirection.Length();
                    lightDirection = Vector3.Normalize(lightDirection);

                    var shadowRay = new RaySIMD(hitPosition, lightDirection);
                    if (
                        scene.Meshes.Intersect(shadowRay, out var shadowHit)
                        && shadowHit.Distance < distanceToLight
                    )
                        return Vector3.Zero;

                    var r = Vector3.Reflect(-lightDirection, normal);
                    var nDotL = MathF.Max(Vector3.Dot(normal, lightDirection), 0);
                    var rDotV = MathF.Pow(MathF.Max(Vector3.Dot(r, v), 0), SpecExponent);

                    var diffuse = OneOverPi * lightColor * surfaceColor * nDotL;
                    var specular = lightColor * Vector3.One * rDotV;

                    return DiffuseStrength * diffuse + SpecularStrength * specular;
                }
            );

            return color;
        }
    }
}
