// RayTracingGlobal.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

namespace PhotonLab.Source.RayTracing;

public static class RayTracingGlobal
{
    public const float IntersectionEpsilon = 1e-4f;
    public const float HitOffsetEpsilon = 1e-1f;
    public const int MaxRecursion = 20;
}
