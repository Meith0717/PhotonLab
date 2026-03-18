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
                float sinTheta = MathF.Sin(theta),
                    cosTheta = MathF.Cos(theta);

                for (var x = 0; x <= segments; x++)
                {
                    var u = x / (float)segments;
                    var phi = u * MathF.PI * 2f;
                    float sinPhi = MathF.Sin(phi),
                        cosPhi = MathF.Cos(phi);

                    // Normal for a sphere at origin is just the direction from center to point
                    var normal = new Vector3(sinTheta * cosPhi, cosTheta, sinTheta * sinPhi);
                    var pos = normal * radius;

                    vertices.Add(new VertexPositionNormalTexture(pos, normal, new Vector2(u, v)));
                }
            }

            for (var y = 0; y < rings; y++)
            for (var x = 0; x < segments; x++)
            {
                var first = y * (segments + 1) + x;
                var second = first + segments + 1;
                indices.AddRange([
                    (ushort)first,
                    (ushort)(first + 1),
                    (ushort)second,
                    (ushort)second,
                    (ushort)(first + 1),
                    (ushort)(second + 1),
                ]);
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
                var theta = v * MathF.PI * 0.5f;
                float sinTheta = MathF.Sin(theta),
                    cosTheta = MathF.Cos(theta);

                for (var x = 0; x <= segments; x++)
                {
                    var u = x / (float)segments;
                    var phi = u * MathF.PI * 2f;
                    float sinPhi = MathF.Sin(phi),
                        cosPhi = MathF.Cos(phi);

                    var normal = new Vector3(sinTheta * cosPhi, cosTheta, sinTheta * sinPhi);
                    vertices.Add(
                        new VertexPositionNormalTexture(normal, normal, new Vector2(u, v))
                    );
                }
            }

            for (var y = 0; y < rings; y++)
            for (var x = 0; x < segments; x++)
            {
                var first = y * (segments + 1) + x;
                var second = first + segments + 1;
                indices.AddRange([
                    (ushort)first,
                    (ushort)(first + 1),
                    (ushort)second,
                    (ushort)second,
                    (ushort)(first + 1),
                    (ushort)(second + 1),
                ]);
            }

            return new MeshBody(device, [.. vertices], [.. indices]);
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
                p => new VertexPositionNormalTexture(p, Vector3.Normalize(p), Vector2.Zero)
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

        public static MeshBody CreateTetrahedron(GraphicsDevice device, float side)
        {
            var h = (float)(Math.Sqrt(3) / 2f * side);
            var v0 = new Vector3(0, side, 0);
            var v1 = new Vector3(-side / 2f, 0, -h / 3f);
            var v2 = new Vector3(side / 2f, 0, -h / 3f);
            var v3 = new Vector3(0, 0, 2f * h / 3f);

            var vertices = new List<VertexPositionNormalTexture>();

            // We create 3 vertices per face to ensure sharp normals
            void AddFace(Vector3 a, Vector3 b, Vector3 c)
            {
                var normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
                vertices.Add(new VertexPositionNormalTexture(a, normal, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(b, normal, Vector2.Zero));
                vertices.Add(new VertexPositionNormalTexture(c, normal, Vector2.Zero));
            }

            AddFace(v0, v2, v1);
            AddFace(v0, v1, v3);
            AddFace(v0, v3, v2);
            AddFace(v1, v2, v3);

            ushort[] indices = new ushort[vertices.Count];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = (ushort)i;

            return new MeshBody(device, [.. vertices], indices);
        }

        public static MeshBody CreateQuad(
            GraphicsDevice device,
            float width = 1f,
            float height = 1f
        )
        {
            var hw = width / 2f;
            var hh = height / 2f;
            var normal = Vector3.Backward; // Normal points towards the camera in XNA/MonoGame Z+

            var verts = new[]
            {
                new VertexPositionNormalTexture(
                    new Vector3(-hw, -hh, 0),
                    normal,
                    new Vector2(0, 1)
                ),
                new VertexPositionNormalTexture(new Vector3(hw, -hh, 0), normal, new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(hw, hh, 0), normal, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(-hw, hh, 0), normal, new Vector2(0, 0)),
            };

            return new MeshBody(device, verts, [1, 2, 0, 2, 3, 0]);
        }
    }
}
