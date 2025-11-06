// BasicSolids.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace PhotonLab.Source.Meshes
{
    internal static class BasicSolids
    {
        public static CpuMesh CreateSphere(GraphicsDevice device, int segments = 16, int rings = 16)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<ushort>();

            for (int y = 0; y <= rings; y++)
            {
                float v = y / (float)rings;
                float theta = v * MathF.PI;
                float sinTheta = MathF.Sin(theta), cosTheta = MathF.Cos(theta);

                for (int x = 0; x <= segments; x++)
                {
                    float u = x / (float)segments;
                    float phi = u * MathF.PI * 2f;
                    float sinPhi = MathF.Sin(phi), cosPhi = MathF.Cos(phi);

                    var pos = new Vector3(sinTheta * cosPhi, cosTheta, sinTheta * sinPhi);
                    var n = Vector3.Normalize(pos);
                    vertices.Add(new(pos, n, Vector2.Zero));
                }
            }

            for (int y = 0; y < rings; y++)
                for (int x = 0; x < segments; x++)
                {
                    int first = y * (segments + 1) + x;
                    int second = first + segments + 1;
                    indices.AddRange(new ushort[]
                    {
                        (ushort)first, (ushort)(first + 1), (ushort)second,
                        (ushort)second, (ushort)(first + 1), (ushort)(second + 1)
                    });
                }

            return new CpuMesh(device, [.. vertices], [.. indices]);
        }

        public static CpuMesh CreateTetrahedron(GraphicsDevice device)
        {
            float s = 1f, h = (float)(Math.Sqrt(3) / 2f * s);
            var v = new[]
            {
                new Vector3(0, s, 0),
                new Vector3(-s/2f, 0, -h/3f),
                new Vector3(s/2f, 0, -h/3f),
                new Vector3(0, 0, 2f * h/3f)
            };

            var verts = Array.ConvertAll(v, p => new VertexPositionNormalTexture(p, Vector3.Normalize(p), Vector2.Zero));
            var inds = new ushort[] { 0, 2, 1, 0, 1, 3, 0, 3, 2, 1, 2, 3 };

            return new CpuMesh(device, verts, inds);
        }

        public static CpuMesh CreateCube(GraphicsDevice device)
        {
            float s = 0.5f;
            var v = new[]
            {
                new Vector3(-s,-s,-s), new(s,-s,-s), new(s,s,-s), new(-s,s,-s),
                new(-s,-s,s), new(s,-s,s), new(s,s,s), new(-s,s,s)
            };

            var verts = Array.ConvertAll(v, p => new VertexPositionNormalTexture(p, Vector3.Normalize(p), Vector2.Zero));
            var inds = new ushort[]
            {
                4,5,6,4,6,7, 1,0,3,1,3,2, 0,4,7,0,7,3,
                5,1,2,5,2,6, 3,7,6,3,6,2, 0,1,5,0,5,4
            };

            return new CpuMesh(device, verts, inds);
        }

        public static CpuMesh CreateQuad(GraphicsDevice device)
        {
            float h = 0.5f;
            var n = new Vector3(0, 0, 1);

            var verts = new[]
            {
                new VertexPositionNormalTexture(new(-h,-h,0), n, Vector2.Zero),
                new VertexPositionNormalTexture(new(h,-h,0), n, Vector2.Zero),
                new VertexPositionNormalTexture(new(h,h,0), n, Vector2.Zero),
                new VertexPositionNormalTexture(new(-h,h,0), n, Vector2.Zero)
            };

            return new CpuMesh(device, verts, new ushort[] { 1, 2, 0, 2, 3, 0 });
        }
    }
}
