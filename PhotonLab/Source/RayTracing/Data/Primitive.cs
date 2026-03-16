// Primitive.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework.Graphics;

namespace PhotonLab.Source.RayTracing.Data;

public readonly struct FacePrimitive(ushort i0, ushort i1, ushort i2)
{
    public readonly ushort I0 = i0;
    public readonly ushort I1 = i1;
    public readonly ushort I2 = i2;
}
