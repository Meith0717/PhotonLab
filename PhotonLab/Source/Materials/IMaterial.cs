// IMaterial.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using PhotonLab.Source.RayTracing;
using System.Numerics;

namespace PhotonLab.Source.Materials
{
    public enum NormalMode { Face, Interpolated }

    internal interface IMaterial
    {
        public CpuTexture2D DiffuseTexture { get; }

        public Vector3 DiffuseColor { get; }

        public Vector3 Shade(Scene scene, int depth, in RaySIMD ray, in HitInfo hit);
    }
}
