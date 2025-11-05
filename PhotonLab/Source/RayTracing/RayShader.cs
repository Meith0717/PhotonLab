// RayShader.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using PhotonLab.Source.Core;

namespace PhotonLab.Source.RayTracing
{
    internal static class RayShader
    {
        public static Color Trace(Scene scene, Ray ray, int depth = 0)
        {
            if (depth > 2 || !scene.Intersect(ray, out var hit))
                return Color.Black;

            return hit.Object.Material.Shade(scene, depth, ray, in hit);
        }
    }
}
