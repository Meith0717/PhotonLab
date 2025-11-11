// LightEmissionPoint.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal readonly struct LightEmissionPoint(Vector3 relativePosition)
    {
        public readonly Vector3 RelativePosition = relativePosition;
    }
}
