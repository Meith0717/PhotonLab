// IBody3D.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Bodies
{
    internal interface IBody3D
    {
        ISurfaceModel SurfaceModel { get; set; }

        Microsoft.Xna.Framework.Matrix ModelTransform { set; }
    }
}
