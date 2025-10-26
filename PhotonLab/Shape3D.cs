// Shape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace PhotonLab
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    internal class Shape3D
    {
        private short[] _indices;
        private VertexPositionColor[] _vertices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;

        public Matrix ModelTransform { get; set; } = Matrix.Identity;

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

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3 );
            }
        }

        public static Shape3D CreateFace(GraphicsDevice graphicsDevice, float size = 1f, Color? color = null)
        {
            var c = color ?? Color.White;
            float h = size / 2f;

            var vertices = new[]
            {
                new VertexPositionColor(new Vector3(0, h, 0), Color.Red),
                new VertexPositionColor(new Vector3(h, -h, 0), Color.Green),
                new VertexPositionColor(new Vector3(-h, -h, 0), Color.Blue)
            };

            var indices = new short[] { 0, 1, 2 };

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices, indices);
            return shape;
        }

        public static Shape3D CreateQuad(GraphicsDevice graphicsDevice, float size = 1f, Color? color = null)
        {
            var c = color ?? Color.White;
            float h = size / 2f;

            var vertices = new[]
            {
                new VertexPositionColor(new Vector3(-h, -h, 0), c),
                new VertexPositionColor(new Vector3(h, -h, 0), c),
                new VertexPositionColor(new Vector3(h, h, 0), c),
                new VertexPositionColor(new Vector3(-h, h, 0), c)
            };

            var indices = new short[]
            {       
                0, 1, 2,
                0, 2, 3
            };

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices, indices);
            return shape;
        }

        public static Shape3D CreateTetrahedron(GraphicsDevice graphicsDevice, float size = 1f)
        {
            float h = size / (2f * (float)Math.Sqrt(2));

            var vertices = new VertexPositionColor[]
            {
                new(new Vector3( h,  h,  h), Color.Red),
                new(new Vector3(-h, -h,  h), Color.Green),
                new(new Vector3(-h,  h, -h), Color.Blue),
                new(new Vector3( h, -h, -h), Color.Yellow)
            };

            var indices = new short[]
            {
                0, 1, 2,
                0, 1, 3,
                0, 2, 3,
                1, 2, 3,
            };

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices, indices);
            return shape;
        }

        public static Shape3D CreateSphere(GraphicsDevice graphicsDevice, float radius = 1f, int segments = 16, int rings = 16, Color? color = null)
        {
            var c = color ?? Color.White;
            var random = new Random();
            var vertices = new List<VertexPositionColor>();
            var indices = new List<short>();

            // Generate vertices
            for (int y = 0; y <= rings; y++)
            {
                float v = (float)y / rings;
                float theta = v * MathF.PI; // from 0 to π

                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int x = 0; x <= segments; x++)
                {
                    float u = (float)x / segments;
                    float phi = u * MathF.PI * 2f; // from 0 to 2π

                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float px = radius * sinTheta * cosPhi;
                    float py = radius * cosTheta;
                    float pz = radius * sinTheta * sinPhi;

                    var randomColor = new Vector3(random.Next(0, 2), random.Next(0, 2), random.Next(0, 2)) * 255;
                    vertices.Add(new(new(px, py, pz), new Color(randomColor)));
                }
            }

            // Generate indices
            for (int y = 0; y < rings; y++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int first = (y * (segments + 1)) + x;
                    int second = first + segments + 1;

                    indices.Add((short)first);
                    indices.Add((short)second);
                    indices.Add((short)(first + 1));

                    indices.Add((short)(second));
                    indices.Add((short)(second + 1));
                    indices.Add((short)(first + 1));
                }
            }

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, [.. vertices], [.. indices]);
            return shape;
        }

    }
}
