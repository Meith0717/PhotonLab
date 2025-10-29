// TestMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace PhotonLab
{
    internal class DefaultMaterial : IMaterial
    {
        public Color Shade(Scene scene, int depth, Ray ray, in HitInfo hit)
        {
            Vector3 color = Vector3.Zero;

            foreach (var light in scene.Lights)
            {
                var toLightDir = Vector3.Normalize(light.Position - hit.Position);
                var hitPos = hit.Position + hit.Normal * .1e-5f;
                var shadowRay = new Ray(hitPos, toLightDir);

                if (scene.Intersect(shadowRay, out var _))
                    continue;

                float nDotL = MathF.Max(Vector3.Dot(hit.Normal, toLightDir), 0f);
                color += light.Color.ToVector3() * hit.ReflectanceColor.ToVector3() * nDotL;
            }

            return new Color(color);
        }
    }
}
