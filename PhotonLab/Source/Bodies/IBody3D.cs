// IBody3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using PhotonLab.Source.Materials;
using PhotonLab.Source.RayTracing;

namespace PhotonLab.Source.Bodies
{
    internal interface IBody3D
    {
        IMaterial Material { get; set; }

        Microsoft.Xna.Framework.Matrix ModelTransform { set; }

        bool Intersect(in RaySIMD ray, out HitInfo hit);
    }
}
