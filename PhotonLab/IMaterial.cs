// IMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal interface IMaterial
    {
        public Vector3 Shade(Scene scene, Ray ray, RayHit hit, int depth);
    }
}
