// Sampling.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Numerics;

namespace PhotonLab.Source.RayTracing.Utilities
{
    /// <summary>
    /// Sampling utilities for BSDF implementations.
    /// </summary>
    internal static class Sampling
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generate a random float in [0, 1).
        /// </summary>
        public static float RandomFloat()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>
        /// Cosine-weighted hemisphere sampling.
        /// </summary>
        public static Vector3 CosineSampleHemisphere(out float pdf)
        {
            var u1 = RandomFloat();
            var u2 = RandomFloat();

            // Concentric map disk sampling
            var r = MathF.Sqrt(u1);
            var theta = 2.0f * MathF.PI * u2;

            var x = r * MathF.Cos(theta);
            var y = r * MathF.Sin(theta);
            var z = MathF.Sqrt(MathF.Max(0.0f, 1.0f - x * x - y * y));

            pdf = z * (1.0f / MathF.PI);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// GGX importance sampling for microfacet BSDFs.
        /// </summary>
        public static Vector3 GGXSampleHemisphere(float alpha, out float pdf)
        {
            var u1 = RandomFloat();
            var u2 = RandomFloat();

            // Transform to spherical coordinates
            var theta = MathF.Atan(alpha * MathF.Sqrt(u1) / MathF.Sqrt(1.0f - u1));
            var phi = 2.0f * MathF.PI * u2;

            // Convert to Cartesian coordinates
            var sinTheta = MathF.Sin(theta);
            var cosTheta = MathF.Cos(theta);

            var x = sinTheta * MathF.Cos(phi);
            var y = sinTheta * MathF.Sin(phi);
            var z = cosTheta;

            // Calculate PDF
            var cosTheta2 = z * z;
            var tanTheta2 = (1.0f - cosTheta2) / cosTheta2;
            var cosTheta4 = cosTheta2 * cosTheta2;

            var denom = MathF.PI * cosTheta4 * (tanTheta2 * alpha * alpha + 1.0f);
            denom = denom * denom;

            pdf = (alpha * alpha * cosTheta) / denom;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Uniform hemisphere sampling.
        /// </summary>
        public static Vector3 UniformSampleHemisphere(out float pdf)
        {
            var u1 = RandomFloat();
            var u2 = RandomFloat();

            var z = u1;
            var r = MathF.Sqrt(MathF.Max(0.0f, 1.0f - z * z));
            var phi = 2.0f * MathF.PI * u2;

            var x = r * MathF.Cos(phi);
            var y = r * MathF.Sin(phi);

            pdf = 1.0f / (2.0f * MathF.PI);
            return new Vector3(x, y, z);
        }
    }
}
