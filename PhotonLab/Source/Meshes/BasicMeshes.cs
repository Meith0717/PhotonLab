// BasicMeshes.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PhotonLab.Source.Meshes
{
    internal static class BasicMeshes
    {
        public static CpuMesh CreateQuad(GraphicsDevice graphicsDevice)
        {
            float h = 1f / 2f;

            var p0 = new Vector3(-h, -h, 0);
            var p1 = new Vector3(h, -h, 0);
            var p2 = new Vector3(h, h, 0);
            var p3 = new Vector3(-h, h, 0);
            var normal = new Vector3(0, 0, 1);

            var vertices = new VertexPositionNormalTexture[]
            {
                new(p0, normal, Vector2.Zero),
                new(p1, normal, Vector2.Zero),
                new(p2, normal, Vector2.Zero),
                new(p3, normal, Vector2.Zero)
            };

            return new CpuMesh(graphicsDevice, vertices, [0, 2, 1, 0, 3, 2]);
        }

        public static CpuMesh CreateCube(GraphicsDevice graphicsDevice, Color? color = null, bool clockwise = true)
        {
            var c = color ?? Color.White;
            float s = 0.5f;

            var center = Vector3.Zero;
            var v0 = new Vector3(-s, -s, -s);
            var v1 = new Vector3(s, -s, -s);
            var v2 = new Vector3(s, s, -s);
            var v3 = new Vector3(-s, s, -s);
            var v4 = new Vector3(-s, -s, s);
            var v5 = new Vector3(s, -s, s);
            var v6 = new Vector3(s, s, s);
            var v7 = new Vector3(-s, s, s);

            var vertices = new VertexPositionNormalTexture[]
            {
                new(v0, Vector3.Normalize(v0 - center), Vector2.Zero),
                new(v1, Vector3.Normalize(v1 - center), Vector2.Zero),
                new(v2, Vector3.Normalize(v2 - center), Vector2.Zero),
                new(v3, Vector3.Normalize(v3 - center), Vector2.Zero),
                new(v4, Vector3.Normalize(v4 - center), Vector2.Zero),
                new(v5, Vector3.Normalize(v5 - center), Vector2.Zero),
                new(v6, Vector3.Normalize(v6 - center), Vector2.Zero),
                new(v7, Vector3.Normalize(v7 - center), Vector2.Zero),
            };

            var indices = new ushort[]
            {
                4, 5, 6,  4, 6, 7,
                1, 0, 3,  1, 3, 2,
                0, 4, 7,  0, 7, 3,
                5, 1, 2,  5, 2, 6,
                3, 7, 6,  3, 6, 2,
                0, 1, 5,  0, 5, 4
            };

            return new CpuMesh(graphicsDevice, vertices, indices);
        }

        public static CpuMesh CreateTetrahedron(GraphicsDevice graphicsDevice, Color? color = null)
        {
            var c = color ?? Color.White;
            var center = Vector3.Zero;

            float size = 1f;
            float h = (float)(Math.Sqrt(3) / 2f * size);

            var v1 = new Vector3(0, size, 0);
            var v2 = new Vector3(-size / 2f, 0, -h / 3f);
            var v3 = new Vector3(size / 2f, 0, -h / 3f);
            var v4 = new Vector3(0, 0, 2f * h / 3f);

            var vertices = new VertexPositionNormalTexture[]
            {
                new(v1, Vector3.Normalize(v1 - center), Vector2.Zero),
                new(v2, Vector3.Normalize(v2 - center), Vector2.Zero),
                new(v3, Vector3.Normalize(v3 - center), Vector2.Zero),
                new(v4, Vector3.Normalize(v4 - center), Vector2.Zero),
            };

            var indices = new ushort[]
            {
                0, 2, 1,
                0, 1, 3,
                0, 3, 2,
                1, 2, 3
            };

            return new CpuMesh(graphicsDevice, vertices, indices);
        }

        public static CpuMesh CreateSphere(GraphicsDevice graphicsDevice, int segments = 16, int rings = 16)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();
            var center = Vector3.Zero;

            for (int y = 0; y <= rings; y++)
            {
                float v = (float)y / rings;
                float theta = v * MathF.PI;

                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int x = 0; x <= segments; x++)
                {
                    float u = (float)x / segments;
                    float phi = u * MathF.PI * 2f;

                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float px = sinTheta * cosPhi;
                    float py = cosTheta;
                    float pz = sinTheta * sinPhi;

                    var vx = new Vector3(px, py, pz);
                    vertices.Add(new(vx, Vector3.Normalize(vx - center), Vector2.Zero));
                }
            }

            for (int y = 0; y < rings; y++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int first = y * (segments + 1) + x;
                    int second = first + segments + 1;

                    indices.Add((ushort)first);
                    indices.Add((ushort)(first + 1));
                    indices.Add((ushort)second);

                    indices.Add((ushort)second);
                    indices.Add((ushort)(first + 1));
                    indices.Add((ushort)(second + 1));
                }
            }

            return new CpuMesh(graphicsDevice, [.. vertices], [.. indices]);
        }
    }
}
