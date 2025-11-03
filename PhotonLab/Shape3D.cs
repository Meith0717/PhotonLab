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
        private readonly BoundingBox _boundingBox;
        private readonly short[] _indices;
        private readonly VertexPositionColorNormal[] _vertices;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        public Matrix ModelTransform { get; set; } = Matrix.Identity;

        public IMaterial Material { get; set; }

        public Shape3D(GraphicsDevice graphicsDevice, VertexPositionColorNormal[] vertices, short[] indices)
        {
            _vertices = vertices;
            _indices = indices;

            _boundingBox = new()
            {
                Min = new Vector3(float.MaxValue),
                Max = new Vector3(float.MinValue)
            };

            foreach (var vertex in _vertices)
            {
                _boundingBox.Min = Vector3.Min(_boundingBox.Min, vertex.Position);
                _boundingBox.Max = Vector3.Max(_boundingBox.Max, vertex.Position);
            }

            _vertexBuffer?.Dispose();
            _vertexBuffer = new(graphicsDevice, typeof(VertexPositionColorNormal), _vertices.Length, BufferUsage.None);
            _vertexBuffer.SetData(_vertices);

            _indexBuffer?.Dispose();
            _indexBuffer = new(graphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.None);
            _indexBuffer.SetData(_indices);
        }

        public bool Intersect(Ray ray, out HitInfo hit)
        {
            hit = default;
            var anyHit = false;

            if (!_boundingBox.IntersectsRay(ref ray, ModelTransform, out var _))
                return anyHit;

            var minT = float.MaxValue;

            for (int i = 0; i < _indices.Length; i += 3)
            {
                var v0 = _vertices[_indices[i]];
                var v1 = _vertices[_indices[i + 1]];
                var v2 = _vertices[_indices[i + 2]];

                var p0 = Vector3.Transform(v0.Position, ModelTransform);
                var p1 = Vector3.Transform(v1.Position, ModelTransform);
                var p2 = Vector3.Transform(v2.Position, ModelTransform);

                if (!ray.IntersectsFace((p0, p1, p2), out var coordinates) || coordinates.T >= minT)
                    continue;

                minT = coordinates.T; anyHit = true;
                hit = new(
                    coordinates.T,
                    ray.Position + ray.Direction * coordinates.T,
                    Vector3.Normalize(Vector3.TransformNormal(coordinates.InterpolateVector3(v0.Normal, v1.Normal, v2.Normal), ModelTransform)),
                    coordinates.InterpolateColor(v0.Color, v1.Color, v2.Color),
                    this);
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

            DrawBoundingBox(graphicsDevice, basicEffect, Color.Red);
        }

        public void DrawBoundingBox(GraphicsDevice graphicsDevice, BasicEffect effect, Color color)
        {
            var corners = _boundingBox.GetCorners();

            int[] indices =
            [
                0, 1, 1, 2, 2, 3, 3, 0, // Bottom
                4, 5, 5, 6, 6, 7, 7, 4, // Top
                0, 4, 1, 5, 2, 6, 3, 7  // Vertical
            ];

            var lines = new VertexPositionColor[8];
            for (int i = 0; i < 8; i++)
                lines[i] = new VertexPositionColor(corners[i], color);

            effect.World = ModelTransform; // Apply your model transform
            effect.VertexColorEnabled = true;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.LineList,
                    lines, 0, 8,
                    indices, 0, 12
                );
            }
        }

        private static void MaybeFlip(ref short a, ref short b, ref short c, bool clockwise)
        {
            if (!clockwise)
                (b, c) = (c, b);
        }

        public static Shape3D CreateQuad(GraphicsDevice graphicsDevice, Color? color = null, bool clockwise = true)
        {
            var c = color ?? Color.White;
            float h = 1f / 2f;

            var p0 = new Vector3(-h, -h, 0);
            var p1 = new Vector3(h, -h, 0);
            var p2 = new Vector3(h, h, 0);
            var p3 = new Vector3(-h, h, 0);
            var normal = new Vector3(0, 0, -1);

            if (!clockwise)
                normal = -normal;

            var vertices = new VertexPositionColorNormal[]
            {
                new(p0, c, normal),
                new(p1, c, normal),
                new(p2, c, normal),
                new(p3, c, normal)
            };

            short a1 = 0, b1 = 2, c1 = 1;
            short a2 = 0, b2 = 3, c2 = 2;
            MaybeFlip(ref a1, ref b1, ref c1, clockwise);
            MaybeFlip(ref a2, ref b2, ref c2, clockwise);

            var indices = new short[] { a1, b1, c1, a2, b2, c2 };

            return new Shape3D(graphicsDevice, vertices, indices);
        }

        public static Shape3D CreateCube(GraphicsDevice graphicsDevice, Color? color = null, bool clockwise = true)
        {
            var c = color ?? Color.White;
            float s = 0.5f;

            var center = Vector3.Zero;
            var v0 = new Vector3(-s, -s, -s);
            var v1 = new Vector3( s, -s, -s);
            var v2 = new Vector3( s,  s, -s);
            var v3 = new Vector3(-s,  s, -s);
            var v4 = new Vector3(-s, -s,  s);
            var v5 = new Vector3( s, -s,  s);
            var v6 = new Vector3( s,  s,  s);
            var v7 = new Vector3(-s,  s,  s);

            var vertices = new VertexPositionColorNormal[]
            {
                new(v0, c, Vector3.Normalize(v0 - center)),
                new(v1, c, Vector3.Normalize(v1 - center)),
                new(v2, c, Vector3.Normalize(v2 - center)),
                new(v3, c, Vector3.Normalize(v3 - center)),
                new(v4, c, Vector3.Normalize(v4 - center)),
                new(v5, c, Vector3.Normalize(v5 - center)),
                new(v6, c, Vector3.Normalize(v6 - center)),
                new(v7, c, Vector3.Normalize(v7 - center)),
            };

            var indices = new short[]
            {
                4, 5, 6,  4, 6, 7,
                1, 0, 3,  1, 3, 2,
                0, 4, 7,  0, 7, 3,
                5, 1, 2,  5, 2, 6,
                3, 7, 6,  3, 6, 2,
                0, 1, 5,  0, 5, 4
            };

            return new Shape3D(graphicsDevice, vertices, indices);
        }

        public static Shape3D CreateTetrahedron(GraphicsDevice graphicsDevice, Color? color = null)
        {
            var c = color ?? Color.White;
            var center = Vector3.Zero;

            float size = 1f;
            float h = (float)(Math.Sqrt(3) / 2f * size);

            var v1 = new Vector3(0, size, 0);
            var v2 = new Vector3(-size / 2f, 0, -h / 3f);
            var v3 = new Vector3(size / 2f, 0, -h / 3f);
            var v4 = new Vector3(0, 0, 2f * h / 3f);

            var vertices = new VertexPositionColorNormal[]
            {
                new(v1, c, Vector3.Normalize(v1 - center)),
                new(v2, c, Vector3.Normalize(v2 - center)),
                new(v3, c, Vector3.Normalize(v3 - center)),
                new(v4, c, Vector3.Normalize(v4 - center)),
            };

            var indices = new short[]
            {
                0, 2, 1,
                0, 1, 3,
                0, 3, 2, 
                1, 2, 3
            };

            return new Shape3D(graphicsDevice, vertices, indices);
        }


        public static Shape3D CreateSphere(GraphicsDevice graphicsDevice, int segments = 16, int rings = 16, Color? color = null)
        {
            var c = color ?? Color.White;
            var vertices = new List<VertexPositionColorNormal>();
            var indices = new List<short>();
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
                    vertices.Add(new(vx, c, Vector3.Normalize(vx - center)));
                }
            }

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

            return new Shape3D(graphicsDevice, [.. vertices], [.. indices]);
        }
    }
}
