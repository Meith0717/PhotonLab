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

        public CpuTexture2D DiffuseTexture { get; }

        public Vector3 DiffuseColor { get; } = Color.White.ToVector3();
        public Vector3 AmbientColor = Color.White.ToVector3();

        public float DiffuseStrength = 1;
        public float SpecularStrength = 1;
        public float AmbientStrength = 1;
        public float SpecExponent = 10;

        public PhongMaterial(Texture2D albedo)
        {
            DiffuseTexture = new(albedo);
        }

        public PhongMaterial(Color diffuseColor)
        {
            DiffuseColor = diffuseColor.ToVector3();
        }

        public Vector3 Shade(Scene scene, int depth, Ray ray, in HitInfo hit)
        {
            var textureColor = DiffuseTexture is null ? DiffuseColor : DiffuseTexture.SampleData(hit.TexturePos).ToVector3();
            var color = OneOverPi * AmbientStrength * AmbientColor * textureColor;

            var hitNormal = hit.Normal;
            //if (Vector3.Dot(hitNormal, -ray.Direction) < -1)
            //    hitNormal = -hitNormal;

            var hitPosition = ray.Position + ray.Direction * hit.Distance;
            hitPosition += hitNormal * Epsilon;
            var v = Vector3.Normalize(scene.Camer3D.Position - hitPosition);

            foreach (var light in scene.Lights)
            {
                var toLightDir = Vector3.Normalize(light.Position - hitPosition);
                var lightDist = Vector3.Distance(light.Position, hitPosition);
                var shadowRay = new Ray(hitPosition, toLightDir);
                if (scene.Intersect(shadowRay, out var shadowHit))
                    if (shadowHit.Distance < lightDist && shadowHit.Distance > Epsilon)
                        continue;

                var r = Vector3.Reflect(-toLightDir, hitNormal);
                float nDotL = MathF.Max(Vector3.Dot(hitNormal, toLightDir), 0);
                float rDotV = MathF.Pow(MathF.Max(Vector3.Dot(r, v), 0), SpecExponent);

                var diffuse = light.Color * textureColor * nDotL;
                var specular = light.Color * Color.White.ToVector3() * rDotV;

                color += DiffuseStrength * diffuse + SpecularStrength * specular;
            }

            return color;
        }
    }
}
