// PhongMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.RayTracing;
using System;
using System.Numerics;

namespace PhotonLab.Source.Materials
{
    internal class PhongMaterial : IMaterial
    {
        private const float OneOverPi = 1 / float.Pi;
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
            DiffuseTexture = new(albedo);
            _normalMode = normalMode;
        }

        public PhongMaterial(Microsoft.Xna.Framework.Color diffuseColor, NormalMode normalMode = NormalMode.Interpolated)
        {
            DiffuseColor = diffuseColor.ToVector3().ToNumerics();
            _normalMode = normalMode;
        }

        public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit)
        {
            var textureColor = DiffuseTexture is null ? DiffuseColor : DiffuseTexture.SampleData3(hit.TexturePos);
            var color = OneOverPi * AmbientStrength * AmbientColor * textureColor;

            var n = _normalMode switch
            {
                NormalMode.Face => hit.FaceNormal,
                NormalMode.Interpolated => hit.InterpolatedNormal,
                _ => throw new NotImplementedException()
            };

            var hitPosition = ray.Position + ray.Direction * hit.Distance;
            hitPosition += n * IMaterial.Epsilon;
            var v = Vector3.Normalize(scene.Camer3D.Position.ToNumerics() - hitPosition);

            foreach (var lightSource in scene.LightSources)
            {
                var lightInfos = lightSource.GetLightInfos(scene, hitPosition, IMaterial.Epsilon);
                foreach (var lightInfo in lightInfos)
                {
                    var r = Vector3.Reflect(-lightInfo.Direction, n);
                    float nDotL = MathF.Max(Vector3.Dot(n, lightInfo.Direction), 0);
                    float rDotV = MathF.Pow(MathF.Max(Vector3.Dot(r, v), 0), SpecExponent);

                    var diffuse = OneOverPi * lightInfo.Color * textureColor * nDotL;
                    var specular = lightInfo.Color * Vector3.One * rDotV;

                    color += DiffuseStrength * diffuse + SpecularStrength * specular;
                }
            }

            return color;
        }
    }
}
