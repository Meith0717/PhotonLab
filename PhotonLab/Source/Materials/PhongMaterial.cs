// PhongMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Core;
using PhotonLab.Source.RayTracing;
using System;

namespace PhotonLab.Source.Materials
{
    internal class PhongMaterial : IMaterial
    {
        private const float Epsilon = .1e-1f;
        private const float OneOverPi = 1 / float.Pi;
        private readonly NormalMode _normalMode;

        public CpuTexture2D DiffuseTexture { get; }

        public Vector3 DiffuseColor { get; } = Color.White.ToVector3();
        public Vector3 AmbientColor = Color.White.ToVector3();

        public float DiffuseStrength = 1;
        public float SpecularStrength = 1;
        public float AmbientStrength = 1;
        public float SpecExponent = 40;

        public PhongMaterial(Texture2D albedo, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseTexture = new(albedo);
            _normalMode = normalMode;
        }

        public PhongMaterial(Color diffuseColor, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseColor = diffuseColor.ToVector3();
            _normalMode = normalMode;
        }

        public Vector3 Shade(Scene scene, int depth, Ray ray, in HitInfo hit)
        {
            var textureColor = DiffuseTexture is null ? DiffuseColor : DiffuseTexture.SampleData(hit.TexturePos).ToVector3();
            var color = OneOverPi * AmbientStrength * AmbientColor * textureColor;

            var n = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException()
            };

            var hitPosition = ray.Position + ray.Direction * hit.Distance;
            hitPosition += n * Epsilon;
            var v = Vector3.Normalize(scene.Camer3D.Position - hitPosition);

            foreach (var lightSource in scene.LightSources)
            {
                var lightInfos = lightSource.GetLightInfos(scene, hitPosition, Epsilon);
                foreach (var lightInfo in lightInfos)
                {
                    var r = Vector3.Reflect(-lightInfo.Direction, n);
                    float nDotL = MathF.Max(Vector3.Dot(n, lightInfo.Direction), 0);
                    float rDotV = MathF.Pow(MathF.Max(Vector3.Dot(r, v), 0), SpecExponent);

                    var diffuse = lightInfo.Color * textureColor * nDotL;
                    var specular = lightInfo.Color * Color.White.ToVector3() * rDotV;

                    color += DiffuseStrength * diffuse + SpecularStrength * specular;
                }
            }

            return color;
        }
    }
}
