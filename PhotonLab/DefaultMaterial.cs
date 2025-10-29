// TestMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace PhotonLab
{
    internal class DefaultMaterial : IMaterial
    {
        private const float Epsilon = .1e-4f;

        public Color Shade(Scene scene, int depth, Ray ray, in HitInfo hit)
        {
            Vector3 color = Vector3.Zero;

            foreach (var light in scene.Lights)
            {
                var toLightDir = Vector3.Normalize(light.Position - hit.Position);
                var hitPos = hit.Position + hit.Normal * Epsilon;
                var shadowRay = new Ray(hitPos, toLightDir);

                if (scene.Intersect(shadowRay, out var shadowHit) && shadowHit.Distance <= Epsilon)
                    continue;

                float nDotL = MathF.Max(Vector3.Dot(hit.Normal, toLightDir), 0f);
                color += light.Color.ToVector3() * hit.ReflectanceColor.ToVector3() * nDotL;
            }

            return new Color(color);
        }
    }
}
