// PhongModel.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.RayTracing.Data;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Materials
{
    internal class PhongModel : ISurfaceModel, IColoredSurface, ITexturedSurface
    {
        private const float OneOverPi = 1; // 1 / float.Pi;

        public CpuTexture2D Texture { get; }
        public Color Color { get; } = Color.White;
        public NormalMode NormalMode { get; }

        private readonly float _diffuseStrength;
        private readonly float _specularStrength;
        private readonly float _specularExponent;

        public PhongModel(
            Texture2D albedo,
            NormalMode normalMode,
            float diffuseStrength,
            float specularStrength,
            float specularExponent
        )
        {
            Texture = new CpuTexture2D(albedo);
            NormalMode = normalMode;
            _diffuseStrength = diffuseStrength;
            _specularStrength = specularStrength;
            _specularExponent = specularExponent;
        }

        public PhongModel(
            Color color,
            NormalMode normalMode,
            float diffuseStrength,
            float specularStrength,
            float specularExponent
        )
        {
            Color = color;
            NormalMode = normalMode;
            _diffuseStrength = diffuseStrength;
            _specularStrength = specularStrength;
            _specularExponent = specularExponent;
        }

        public Radiance Shade(
            Scene scene,
            int depth,
            in RaySimd ray,
            in SurfaceIntersectionData surfaceData
        )
        {
            var normal = surfaceData.Normal;
            var surfaceColor = Texture?.SampleData(surfaceData.TexturePos) ?? Color;

            var radiance = new Radiance(surfaceColor)
                .Attenuate(scene.AmbientColor, scene.AmbientIntensity)
                .Attenuate(OneOverPi);

            var v = -ray.Direction;
            radiance += scene.LightSources.Forall(
                scene,
                in surfaceData,
                (lightRadiance, lightDirection) =>
                {
                    var diffuseRadiance = lightRadiance
                        .Attenuate(surfaceColor, _diffuseStrength)
                        .Attenuate(OneOverPi)
                        .CosLaw(lightDirection, normal);

                    if (_specularStrength == 0)
                        return diffuseRadiance;

                    var r = Vector3.Reflect(-lightDirection, normal);
                    var rDotV = float.Pow(float.Max(Vector3.Dot(r, v), 0), _specularExponent);
                    var specularRadiance = lightRadiance
                        .Attenuate(rDotV)
                        .Attenuate(_specularStrength);

                    return diffuseRadiance + specularRadiance;
                }
            );

            return radiance;
        }
    }
}
