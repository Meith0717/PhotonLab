// Shape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace PhotonLab.Source.Core
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Shape3D : IShape3D
    {
        private readonly ushort[] _indices;
        private readonly VertexPositionNormalTexture[] _vertices;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private readonly BoundingBox _boundingBox;

        public Matrix ModelTransform { get; set; } = Matrix.Identity;

        public Material Material { get; set; }

        public Shape3D(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, ushort[] indices)
        {
            _vertices = vertices;
            _indices = indices;

            _boundingBox = BoundingBox.CreateFromPoints(_vertices.Select(v => v.Position));

            _vertexBuffer = new(graphicsDevice, typeof(VertexPositionNormalTexture), _vertices.Length, BufferUsage.None);
            _vertexBuffer.SetData(_vertices);

            _indexBuffer = new(graphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.None);
            _indexBuffer.SetData(_indices);
        }

        public Shape3D(ModelMeshPart part)
        {
            var vertexCount = part.NumVertices;
            var vertices = new VertexPositionNormalTexture[vertexCount];
            _vertexBuffer = part.VertexBuffer;
            _vertexBuffer.GetData(vertices);

            var indices = new ushort[part.PrimitiveCount * 3];
            _indexBuffer = part.IndexBuffer;
            _indexBuffer.GetData(indices);

            _boundingBox = BoundingBox.CreateFromPoints(vertices.Select(v => v.Position));
            _vertices = [.. vertices];
            _indices = [.. indices];
        }

        public bool Intersect(Ray ray, out HitInfo hit)
        {
            hit = default;
            var anyHit = false;
            var minT = float.MaxValue;

            var localRay = ray.Transform(Matrix.Invert(ModelTransform));
            if (!_boundingBox.IntersectsRay(ref localRay, out var _))
                return anyHit;

            for (int i = 0; i < _indices.Length; i += 3)
            {
                var v0 = _vertices[_indices[i]];
                var v1 = _vertices[_indices[i + 1]];
                var v2 = _vertices[_indices[i + 2]];

                var p0 = Vector3.Transform(v0.Position, ModelTransform);
                var p1 = Vector3.Transform(v1.Position, ModelTransform);
                var p2 = Vector3.Transform(v2.Position, ModelTransform);

                if (ray.IntersectsFace((p0, p1, p2), out var coordinates) && coordinates.T < minT)
                {
                    minT = coordinates.T;
                    var normal = Vector3.Normalize(Vector3.TransformNormal(coordinates.InterpolateVector3(v0.Normal, v1.Normal, v2.Normal), ModelTransform));
                    var texturePos = coordinates.InterpolateVector2(v0.TextureCoordinate, v1.TextureCoordinate, v2.TextureCoordinate);

                    hit = new(coordinates.T, normal, texturePos, this);
                    anyHit = true;
                }
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
            basicEffect.VertexColorEnabled = false;
            if (Material != null && Material.Texture != null)
            {
                basicEffect.Texture = Material.Texture;
                basicEffect.TextureEnabled = true;
            }

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3);
            }

            var corners = _boundingBox.GetCorners();
            int[] indices =
            [
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            ];

            var lines = new VertexPositionColor[8];
            for (int i = 0; i < 8; i++)
                lines[i] = new VertexPositionColor(corners[i], Color.White);

            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, lines, 0, 8, indices, 0, 12);
            }
        }

        private static void MaybeFlip(ref ushort a, ref ushort b, ref ushort c, bool clockwise)
        {
            if (!clockwise)
                (b, c) = (c, b);
        }

        public static Shape3D CreateQuad(GraphicsDevice graphicsDevice, bool clockwise = true)
        {
            float h = 1f / 2f;

            var p0 = new Vector3(-h, -h, 0);
            var p1 = new Vector3(h, -h, 0);
            var p2 = new Vector3(h, h, 0);
            var p3 = new Vector3(-h, h, 0);
            var normal = new Vector3(0, 0, -1);

            if (!clockwise)
                normal = -normal;

            var vertices = new VertexPositionNormalTexture[]
            {
                new(p0, normal, Vector2.Zero),
                new(p1, normal, Vector2.Zero),
                new(p2, normal, Vector2.Zero),
                new(p3, normal, Vector2.Zero)
            };

            ushort a1 = 0, b1 = 2, c1 = 1;
            ushort a2 = 0, b2 = 3, c2 = 2;
            MaybeFlip(ref a1, ref b1, ref c1, clockwise);
            MaybeFlip(ref a2, ref b2, ref c2, clockwise);

            var indices = new ushort[] { a1, b1, c1, a2, b2, c2 };

            return new Shape3D(graphicsDevice, vertices, indices);
        }

        public static Shape3D CreateCube(GraphicsDevice graphicsDevice, Color? color = null, bool clockwise = true)
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

            return new Shape3D(graphicsDevice, vertices, indices);
        }

        public static Shape3D CreateSphere(GraphicsDevice graphicsDevice, int segments = 16, int rings = 16)
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

            return new Shape3D(graphicsDevice, [.. vertices], [.. indices]);
        }
    }
}
