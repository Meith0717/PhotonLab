// ILightSource.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal interface ILightSource
    {
        Vector3 Position { get; }

        Vector3 Color { get; }

        LightEmissionPoint[] Lights { get; }

        LightInfo[] GetLightInfos(Scene scene, Vector3 hitPosition, float epsilon);
    }
}
