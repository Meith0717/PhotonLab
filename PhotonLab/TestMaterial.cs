// TestMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace PhotonLab
{
    internal class TestMaterial : IMaterial
    {
        public Vector3 Shade(Scene scene, Ray ray, RayHit hit, int depth)
        {
            Vector3 color = Vector3.Zero;

            foreach (var light in scene.Lights)
            {
                var toLight = Vector3.Normalize(light.Position - hit.Position);
                var shadowRay = new Ray(hit.Position + hit.Normal * 0.00001f, toLight);

                if (scene.Intersect(shadowRay, out var shadowHit))
                    continue;

                float nDotL = MathF.Max(Vector3.Dot(hit.Normal, toLight), 0f);
                color += light.Color * hit.Color * nDotL;
            }

            return color;
        }
    }
}
