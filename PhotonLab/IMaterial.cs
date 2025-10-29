// IMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace PhotonLab
{
    internal interface IMaterial
    {
        public Color Shade(Scene scene, int depth, Ray ray, in HitInfo hit);
    }
}
