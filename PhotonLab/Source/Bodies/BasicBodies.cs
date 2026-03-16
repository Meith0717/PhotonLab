// BasicBodies.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhotonLab.Source.Bodies
{
    internal static class BasicBodies
    {
        public static MeshBody CreateUvSphere(
            GraphicsDevice device,
            float radius,
            int segments,
            int rings
        )
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            for (var y = 0; y <= rings; y++)
            {
                var v = y / (float)rings;
                var theta = v * MathF.PI;
                float sinTheta = MathF.Sin(theta) * radius,
                    cosTheta = MathF.Cos(theta) * radius;

                for (var x = 0; x <= segments; x++)
                {
                    var u = x / (float)segments;
                    var phi = u * MathF.PI * 2f;
                    float sinPhi = MathF.Sin(phi),
                        cosPhi = MathF.Cos(phi);

                    var pos = new Vector3(sinTheta * cosPhi, cosTheta, sinTheta * sinPhi);
                    vertices.Add(new VertexPositionNormalTexture(pos, Vector3.Zero, Vector2.Zero));
                }
            }

            for (var y = 0; y < rings; y++)
            for (var x = 0; x < segments; x++)
            {
                var first = y * (segments + 1) + x;
                var second = first + segments + 1;
                indices.AddRange(
                    new ushort[]
                    {
                        (ushort)first,
                        (ushort)(first + 1),
                        (ushort)second,
                        (ushort)second,
                        (ushort)(first + 1),
                        (ushort)(second + 1),
                    }
                );
            }

            return new MeshBody(device, [.. vertices], [.. indices]);
        }

        public static MeshBody CreateUvHemisphere(
            GraphicsDevice device,
            int segments = 16,
            int rings = 8
        )
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            for (var y = 0; y <= rings; y++)
            {
                var v = y / (float)rings;

                // Only half the sphere
                var theta = v * MathF.PI * 0.5f;

                float sinTheta = MathF.Sin(theta),
                    cosTheta = MathF.Cos(theta);

                for (var x = 0; x <= segments; x++)
                {
                    var u = x / (float)segments;
                    var phi = u * MathF.PI * 2f;

                    float sinPhi = MathF.Sin(phi),
                        cosPhi = MathF.Cos(phi);

                    var pos = new Vector3(sinTheta * cosPhi, cosTheta, sinTheta * sinPhi);

                    vertices.Add(new VertexPositionNormalTexture(pos, Vector3.Zero, Vector2.Zero));
                }
            }

            for (var y = 0; y < rings; y++)
            for (var x = 0; x < segments; x++)
            {
                var first = y * (segments + 1) + x;
                var second = first + segments + 1;

                indices.AddRange(
                    new ushort[]
                    {
                        (ushort)first,
                        (ushort)(first + 1),
                        (ushort)second,
                        (ushort)second,
                        (ushort)(first + 1),
                        (ushort)(second + 1),
                    }
                );
            }

            return new MeshBody(device, [.. vertices], [.. indices]);
        }

        public static MeshBody CreateTetrahedron(GraphicsDevice device, float side)
        {
            var h = (float)(Math.Sqrt(3) / 2f * side);
            var v = new[]
            {
                new Vector3(0, side, 0),
                new Vector3(-side / 2f, 0, -h / 3f),
                new Vector3(side / 2f, 0, -h / 3f),
                new Vector3(0, 0, 2f * h / 3f),
            };

            var verts = Array.ConvertAll(
                v,
                p => new VertexPositionNormalTexture(p, Vector3.Zero, Vector2.Zero)
            );
            var inds = new ushort[] { 0, 2, 1, 0, 1, 3, 0, 3, 2, 1, 2, 3 };

            return new MeshBody(device, verts, inds);
        }

        public static MeshBody CreateCube(
            GraphicsDevice device,
            float width = 1f,
            float height = 1f,
            float depth = 1f
        )
        {
            var hw = width / 2f;
            var hh = height / 2f;
            var hd = depth / 2f;

            var v = new[]
            {
                new Vector3(-hw, -hh, -hd),
                new(hw, -hh, -hd),
                new(hw, hh, -hd),
                new(-hw, hh, -hd),
                new Vector3(-hw, -hh, hd),
                new(hw, -hh, hd),
                new(hw, hh, hd),
                new(-hw, hh, hd),
            };

            var verts = Array.ConvertAll(
                v,
                p => new VertexPositionNormalTexture(p, Vector3.Zero, Vector2.Zero)
            );

            var inds = new ushort[]
            {
                4,
                5,
                6,
                4,
                6,
                7, // front
                1,
                0,
                3,
                1,
                3,
                2, // back
                0,
                4,
                7,
                0,
                7,
                3, // left
                5,
                1,
                2,
                5,
                2,
                6, // right
                3,
                7,
                6,
                3,
                6,
                2, // top
                0,
                1,
                5,
                0,
                5,
                4, // bottom
            };

            return new MeshBody(device, verts, inds);
        }

        public static MeshBody CreateQuad(GraphicsDevice device)
        {
            var h = 0.5f;
            var verts = new[]
            {
                new VertexPositionNormalTexture(
                    new Vector3(-h, -h, 0),
                    Vector3.Zero,
                    new Vector2(0, 1)
                ),
                new VertexPositionNormalTexture(
                    new Vector3(h, -h, 0),
                    Vector3.Zero,
                    new Vector2(1, 1)
                ),
                new VertexPositionNormalTexture(
                    new Vector3(h, h, 0),
                    Vector3.Zero,
                    new Vector2(1, 0)
                ),
                new VertexPositionNormalTexture(
                    new Vector3(-h, h, 0),
                    Vector3.Zero,
                    new Vector2(0, 0)
                ),
            };

            return new MeshBody(device, verts, [1, 2, 0, 2, 3, 0]);
        }

        public static MeshBody CreateQuad(GraphicsDevice device, float width, float height)
        {
            var halfWidth = width / 2f;
            var halfHeight = height / 2f;

            var verts = new[]
            {
                new VertexPositionNormalTexture(
                    new Vector3(-halfWidth, -halfHeight, 0),
                    Vector3.Zero,
                    new Vector2(0, 1)
                ),
                new VertexPositionNormalTexture(
                    new Vector3(halfWidth, -halfHeight, 0),
                    Vector3.Zero,
                    new Vector2(1, 1)
                ),
                new VertexPositionNormalTexture(
                    new Vector3(halfWidth, halfHeight, 0),
                    Vector3.Zero,
                    new Vector2(1, 0)
                ),
                new VertexPositionNormalTexture(
                    new Vector3(-halfWidth, halfHeight, 0),
                    Vector3.Zero,
                    new Vector2(0, 0)
                ),
            };

            return new MeshBody(device, verts, [1, 2, 0, 2, 3, 0]);
        }
    }
}
