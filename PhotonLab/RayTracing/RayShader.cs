// RayShader.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab.RayTracing
{
    internal static class RayShader
    {
        public static Vector3 Trace(Scene scene, Ray ray, int depth = 0)
        {
            if (depth > 1 || !scene.Intersect(ray, out var hit))
                return Vector3.Zero;

            return hit.Material.Shade(scene, ray, hit, depth);
        }
    }
}
