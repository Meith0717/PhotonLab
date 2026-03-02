// ILightSource.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal interface ILightSource
    {
        Vector3 Position { get; }

        Vector3[] EmissionPoints { get; }

        void GetLightInfo(Vector3 light, Vector3 hitPosition, out LightInfo lightInfo);
    }
}
