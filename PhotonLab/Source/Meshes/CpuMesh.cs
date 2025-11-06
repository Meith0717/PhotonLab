// Shape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace PhotonLab.Source.Meshes
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using PhotonLab.Source.Core;
    using PhotonLab.Source.Materials;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CpuMesh
    {
        // CPU stuff (Ray Tracing)
        private static readonly short[] BoxLineIndices = [0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 2, 6, 3, 7];
        private readonly VertexPositionColor[] _boxVertecies;
        private readonly BoundingBox _boundingBox;

        private readonly ushort[] _primitiveIndices;
        private readonly VertexPositionNormalTexture[] _vertices;

        // GPU stuff (Rasterisation)
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        // Some other Stuff
        public Matrix ModelTransform { get; set; } = Matrix.Identity;
        public IMaterial Material { get; set; }

        public CpuMesh(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, ushort[] indices)
        {
            // Do CPU stuff
            _vertices = vertices;
            _primitiveIndices = indices;
            _boundingBox = BoundingBox.CreateFromPoints(_vertices.Select(v => v.Position));
            var corners = _boundingBox.GetCorners();
            _boxVertecies = new VertexPositionColor[8];
            for (int i = 0; i < 8; i++)
                _boxVertecies[i] = new VertexPositionColor(corners[i], Color.White);

            // Move CPU stuff to GPU
            _vertexBuffer = new(graphicsDevice, typeof(VertexPositionNormalTexture), _vertices.Length, BufferUsage.None);
            _vertexBuffer.SetData(_vertices);
            _indexBuffer = new(graphicsDevice, IndexElementSize.SixteenBits, _primitiveIndices.Length, BufferUsage.None);
            _indexBuffer.SetData(_primitiveIndices);
        }

        public CpuMesh(ModelMesh mesh)
        {
            var buffer = mesh.MeshParts[0].VertexBuffer;
            foreach (var part in mesh.MeshParts)
            {
                if (buffer != part.VertexBuffer)
                    throw new Exception();
            }

            var vertexCount = mesh.MeshParts[0].NumVertices;
            var vertices = new VertexPositionNormalTexture[vertexCount];
            _vertexBuffer = mesh.MeshParts[0].VertexBuffer;
            _vertexBuffer.GetData(vertices);

            var indices = new ushort[mesh.MeshParts[0].PrimitiveCount * 3];
            _indexBuffer = mesh.MeshParts[0].IndexBuffer;
            _indexBuffer.GetData(indices);

            _boundingBox = BoundingBox.CreateFromPoints(vertices.Select(v => v.Position));
            _vertices = [.. vertices];
            _primitiveIndices = [.. indices];
            var corners = _boundingBox.GetCorners();
            _boxVertecies = new VertexPositionColor[8];
            for (int i = 0; i < 8; i++)
                _boxVertecies[i] = new VertexPositionColor(corners[i], Color.White);
        }

        public bool Intersect(Ray ray, out HitInfo hit)
        {
            hit = default;

            var localRay = ray.Transform(Matrix.Invert(ModelTransform));
            if (!_boundingBox.IntersectsRay(ref localRay, out var _))
                return false;

            var anyHit = false;
            var minT = float.MaxValue;

            for (int i = 0; i < _primitiveIndices.Length; i += 3)
            {
                var v0 = _vertices[_primitiveIndices[i]];
                var v1 = _vertices[_primitiveIndices[i + 1]];
                var v2 = _vertices[_primitiveIndices[i + 2]];

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
            if (_vertexBuffer == null || _indexBuffer == null || _vertices == null || _boxVertecies == null ||
                _vertices.Length == 0 || _primitiveIndices == null || _primitiveIndices.Length == 0 || BoxLineIndices.Length == 0)
                return;

            basicEffect.World = ModelTransform;

            // Draw main mesh
            basicEffect.VertexColorEnabled = false;
            basicEffect.DiffuseColor = Vector3.One;
            if (Material is not null)
            {
                if (Material.DiffuseTexture is not null)
                {
                    basicEffect.Texture = Material.DiffuseTexture.Texture2D;
                    basicEffect.TextureEnabled = true;
                }
                else
                {
                    basicEffect.DiffuseColor = Material.DiffuseColor;
                }
            }

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveIndices.Length / 3);
            }

            // Draw Bound box
            basicEffect.TextureEnabled = false;
            basicEffect.VertexColorEnabled = true;
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, _boxVertecies, 0, 8, BoxLineIndices, 0, 12);
            }
        }
    }
}
