// PerfectMirrorModel.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using PhotonLab.Source.RayTracing;
using PhotonLab.Source.Scenes;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Materials
{
    internal class PerfectMirrorModel(NormalMode normalMode) : ISurfaceModel
    {
        public NormalMode NormalMode { get; } = normalMode;

        public Radiance Shade(
            Scene scene,
            int depth,
            in RaySimd ray,
            in SurfaceIntersectionData surfaceData
        )
        {
            var normal = surfaceData.Normal;

            var reflectDir = Vector3.Normalize(Vector3.Reflect(ray.Direction, normal));
            var reflectedRay = new RaySimd(surfaceData.Position, reflectDir);
            var reflectedRadiance = RayTracer.Trace(scene, reflectedRay, depth + 1);

            return reflectedRadiance;
        }
    }
}
