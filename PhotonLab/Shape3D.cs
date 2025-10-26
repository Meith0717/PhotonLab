// Shape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace PhotonLab
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    internal class Shape3D
    {
        private VertexPositionColor[] _vertices;
        private VertexBuffer _vertexBuffer;

        public Matrix ModelTransform { get; set; } = Matrix.Identity;

        public ImmutableArray<VertexPositionColor> GetVertecies() => _vertices.ToImmutableArray();

        public void SetVertecies(GraphicsDevice graphicsDevice, VertexPositionColor[] vertices)
        {
            _vertices = vertices;

            _vertexBuffer?.Dispose();
            _vertexBuffer = new(graphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.None);
            _vertexBuffer.SetData(vertices);
        }

        public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            if (_vertexBuffer == null || _vertices == null || _vertices.Length == 0)
                return;

            basicEffect.World = ModelTransform;
            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertices.Length / 3);
            }
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

                new VertexPositionColor(new Vector3(-h, -h, 0), c),
                new VertexPositionColor(new Vector3(h, h, 0), c),
                new VertexPositionColor(new Vector3(-h, h, 0), c),
            };

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices);
            return shape;
        }

        public static Shape3D CreateTetrahedron(GraphicsDevice graphicsDevice, float size = 1f, Color? color = null)
        {
            var c = color ?? Color.White;
            float scale = size / 2f; // adjust for desired size

            // Regular tetrahedron centered at origin
            var v0 = new Vector3(scale, scale, scale);
            var v1 = new Vector3(-scale, -scale, scale);
            var v2 = new Vector3(-scale, scale, -scale);
            var v3 = new Vector3(scale, -scale, -scale);

            var vertices = new List<VertexPositionColor>();

            // Face 1
            c = Color.Red;
            vertices.Add(new VertexPositionColor(v0, c));
            vertices.Add(new VertexPositionColor(v1, c));
            vertices.Add(new VertexPositionColor(v2, c));

            c = Color.Green; 
            vertices.Add(new VertexPositionColor(v0, c));
            vertices.Add(new VertexPositionColor(v3, c));
            vertices.Add(new VertexPositionColor(v1, c));

            c = Color.Yellow;
            vertices.Add(new VertexPositionColor(v0, c));
            vertices.Add(new VertexPositionColor(v2, c));
            vertices.Add(new VertexPositionColor(v3, c));

            c = Color.White;
            vertices.Add(new VertexPositionColor(v1, c));
            vertices.Add(new VertexPositionColor(v3, c));
            vertices.Add(new VertexPositionColor(v2, c));

            var shape = new Shape3D();
            shape.SetVertecies(graphicsDevice, vertices.ToArray());
            return shape;
        }

    }
}
