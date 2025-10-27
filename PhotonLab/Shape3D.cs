// Shape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace PhotonLab
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Collections.Generic;

    internal class Shape3D : IShape3D
    {
        private short[] _indices;
        private VertexPositionColor[] _vertices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        public Matrix ModelTransform { get; set; } = Matrix.Identity;

        public IMaterial Material => new TestMaterial();

        public void GetVertecies(out VertexPositionColor[] vertecies, out short[] indices)
        {
            vertecies = _vertices;
            indices = _indices;
        }

        public void SetVertecies(GraphicsDevice graphicsDevice, VertexPositionColor[] vertices, short[] indices)
        {
            _vertices = vertices;
            _indices = indices;

            _vertexBuffer?.Dispose();
            _vertexBuffer = new(graphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.None);
            _vertexBuffer.SetData(_vertices);

            _indexBuffer?.Dispose();
            _indexBuffer = new(graphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.None);
            _indexBuffer.SetData(_indices);
        }

        public bool Intersect(Ray ray, out RayHit hit)
        {
            hit = default;
            var anyHit = false;
            var minT = float.MaxValue;
            var bestNormal = Vector3.Zero;
            var color = Vector3.Zero;

            for (int i = 0; i < _indices.Length; i += 3)
            {
                var v0 = _vertices[_indices[i]];
                var v1 = _vertices[_indices[i + 1]];
                var v2 = _vertices[_indices[i + 2]];

                var p0 = Vector3.Transform(v0.Position, ModelTransform);
                var p1 = Vector3.Transform(v1.Position, ModelTransform);
                var p2 = Vector3.Transform(v2.Position, ModelTransform);

                if (!ray.IntersectsFace((p0, p1, p2), out var b0, out var b1, out var b2, out var t) || t >= minT)
                    continue;

                minT = t;
                anyHit = true;
                bestNormal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));
                color = b0 * v0.Color.ToVector3() + b1 * v1.Color.ToVector3() + b2 * v2.Color.ToVector3();
            }

            if (anyHit)
            {
                hit.Hit = true;
                hit.Distance = minT;
                hit.Color = color;
                hit.Position = ray.Position + ray.Direction * minT;
                hit.Normal = bestNormal;
                hit.Material = Material;
            }

            return anyHit;
        }

        public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            if (_vertexBuffer == null ||
                _indexBuffer == null ||
                _vertices == null || _vertices.Length == 0 ||
                _indices == null || _indices.Length == 0)
                return;

            basicEffect.World = ModelTransform;

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3);
            }
        }

        private static void MaybeFlip(ref short a, ref short b, ref short c, bool clockwise)
        {
            if (!clockwise)
                (b, c) = (c, b);
        }


        public static Shape3D CreateFace(GraphicsDevice graphicsDevice, float size = 1f, Color? color = null, bool clockwise = true)
        {
            var c = color ?? Color.White;
            float h = size / 2f;

            var vertices = new[]
            {
                new VertexPositionColor(new Vector3(0,  h, 0), Color.Red),
                new VertexPositionColor(new Vector3(-h, -h, 0), Color.Green),
                new VertexPositionColor(new Vector3( h, -h, 0), Color.Blue)
            };

            short a = 0, b = 1, cIdx = 2;
            MaybeFlip(ref a, ref b, ref cIdx, clockwise);

            var indices = new short[] { a, b, cIdx };

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices, indices);
            return shape;
        }

        public static Shape3D CreateQuad(GraphicsDevice graphicsDevice, Color? color = null, bool clockwise = true)
        {
            var c = color ?? Color.White;
            float h = 1 / 2f;

            var vertices = new[]
            {
                new VertexPositionColor(new Vector3(-h, -h, 0), c),
                new VertexPositionColor(new Vector3( h, -h, 0), c),
                new VertexPositionColor(new Vector3( h,  h, 0), c),
                new VertexPositionColor(new Vector3(-h,  h, 0), c)
            };

            short a1 = 0, b1 = 2, c1 = 1;
            short a2 = 0, b2 = 3, c2 = 2;
            MaybeFlip(ref a1, ref b1, ref c1, clockwise);
            MaybeFlip(ref a2, ref b2, ref c2, clockwise);

            var indices = new short[] { a1, b1, c1, a2, b2, c2 };

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices, indices);
            return shape;
        }

        public static Shape3D CreateTetrahedron(GraphicsDevice graphicsDevice, float size = 1f)
        {
            float h = size / (2f * (float)Math.Sqrt(2));

            var vertices = new VertexPositionColor[]
            {
                new(new Vector3( h,  h,  h), Color.Red),     // 0
                new(new Vector3(-h, -h,  h), Color.Green),   // 1
                new(new Vector3(-h,  h, -h), Color.Blue),    // 2
                new(new Vector3( h, -h, -h), Color.Yellow)   // 3
            };

            var indices = new short[]
            {
                0, 2, 1,
                0, 1, 3,
                0, 3, 2,
                1, 2, 3 
            };

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices, indices);
            return shape;
        }

        public static Shape3D CreateSphere(GraphicsDevice graphicsDevice, float radius = 1f, int segments = 16, int rings = 16, Color? color = null)
        {
            var c = color ?? Color.White;
            var vertices = new List<VertexPositionColor>();
            var indices = new List<short>();

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

                    float px = radius * sinTheta * cosPhi;
                    float py = radius * cosTheta;
                    float pz = radius * sinTheta * sinPhi;

                    vertices.Add(new(new(px, py, pz), c));
                }
            }

            // Clockwise winding per quad
            for (int y = 0; y < rings; y++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int first = (y * (segments + 1)) + x;
                    int second = first + segments + 1;

                    indices.Add((short)first);
                    indices.Add((short)(first + 1));
                    indices.Add((short)second);

                    indices.Add((short)(second));
                    indices.Add((short)(first + 1));
                    indices.Add((short)(second + 1));
                }
            }

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, [.. vertices], [.. indices]);
            return shape;
        }
    }
}
