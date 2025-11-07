// Shape3D.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

namespace PhotonLab.Source.Meshes
{
    using Microsoft.Xna.Framework.Graphics;
    using PhotonLab.Source.Core;
    using PhotonLab.Source.Materials;
    using PhotonLab.Source.RayTracing;
    using System;
    using System.Numerics;
    using System.Threading.Tasks;

    internal class CpuMesh
    {
        // CPU stuff (Ray Tracing)
        private readonly BoundingBoxSIMD _boundingBox;

        private readonly ushort[] _primitiveIndices;
        private readonly Vector3[] _vertexPositions;
        private readonly Vector3[] _vertexNormals;
        private readonly Vector2[] _vertexTextures;
        private Matrix4x4 _transform;
        private Matrix4x4 _invTransform;

        // GPU stuff (Rasterisation)
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        // Some other Stuff
        public IMaterial Material { get; set; }
        public Microsoft.Xna.Framework.Matrix ModelTransform 
        {
            set { 
                _transform = value.ToNumerics(); 
                Matrix4x4.Invert(_transform, out _invTransform);
            }
        }

        public CpuMesh(GraphicsDevice graphicsDevice, VertexPositionNormalTexture[] vertices, ushort[] indices)
        {
            var vertexCount = vertices.Length;
            _vertexPositions = new Vector3[vertexCount];
            _vertexNormals = new Vector3[vertexCount];
            _vertexTextures = new Vector2[vertexCount];
            Parallel.For(0, vertexCount, i =>
            {
                _vertexPositions[i] = vertices[i].Position.ToNumerics();
                _vertexNormals[i] = vertices[i].Normal.ToNumerics();
                _vertexTextures[i] = vertices[i].TextureCoordinate.ToNumerics();
            });

            _primitiveIndices = indices;
            _boundingBox = BoundingBoxSIMD.CreateFromPoints(_vertexPositions);

            _vertexBuffer = new(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
            _vertexBuffer.SetData(vertices);
            _indexBuffer = new(graphicsDevice, IndexElementSize.SixteenBits, _primitiveIndices.Length, BufferUsage.None);
            _indexBuffer.SetData(_primitiveIndices);
        }

        public CpuMesh(ModelMesh mesh)
        {
            var mainMesh = mesh.MeshParts[0];

            foreach (var part in mesh.MeshParts)
                if (mainMesh.VertexBuffer != part.VertexBuffer)
                    throw new Exception();

            var vertexCount = mainMesh.NumVertices;
            var primitiveCount = mainMesh.PrimitiveCount;

            // Load iindex data from GPU
            _primitiveIndices = new ushort[primitiveCount * 3];
            _indexBuffer.GetData(_primitiveIndices);

            // Load vertex data from GPU
            _vertexPositions = new Vector3[vertexCount];
            _vertexNormals = new Vector3[vertexCount];
            _vertexTextures = new Vector2[vertexCount];
            var vertexBufferData = new VertexPositionNormalTexture[vertexCount];
            _vertexBuffer.GetData(vertexBufferData);
            Parallel.For(0, vertexCount, i =>
            {
                _vertexPositions[i] = vertexBufferData[i].Position.ToNumerics();
                _vertexNormals[i] = vertexBufferData[i].Normal.ToNumerics();
                _vertexTextures[i] = vertexBufferData[i].TextureCoordinate.ToNumerics();
            });

            _boundingBox = BoundingBoxSIMD.CreateFromPoints(_vertexNormals);

            _indexBuffer = mainMesh.IndexBuffer;
            _vertexBuffer = mainMesh.VertexBuffer;
        }

        public bool Intersect(in RaySIMD ray, out HitInfo hit)
        {
            hit = default;

            var localRay = ray.Transform(_invTransform);
            if (!_boundingBox.IntersectsRay(ref localRay, out var _))
                return false;

            var anyHit = false;
            var minT = float.MaxValue;

            for (int i = 0; i < _primitiveIndices.Length; i += 3)
            {
                var i0 = _primitiveIndices[i];
                var i1 = _primitiveIndices[i + 1];
                var i2 = _primitiveIndices[i + 2];

                var p0 = Vector3.Transform(_vertexPositions[i0], _transform);
                var p1 = Vector3.Transform(_vertexPositions[i1], _transform);
                var p2 = Vector3.Transform(_vertexPositions[i2], _transform);

                var n0 = _vertexNormals[i0];
                var n1 = _vertexNormals[i1];
                var n2 = _vertexNormals[i2];

                var t0 = _vertexTextures[i0];
                var t1 = _vertexTextures[i1];
                var t2 = _vertexTextures[i2];

                if (ray.IntersectsFace((p0, p1, p2), out var coordinates) && coordinates.T < minT)
                {
                    minT = coordinates.T;
                    var normal = Vector3.Normalize(Vector3.TransformNormal(coordinates.InterpolateVector3(n0, n1, n2), _transform));
                    var faceNormal = Vector3.Normalize(Vector3.Cross(p1 - p0, p2 - p0));
                    var texturePos = coordinates.InterpolateVector2(t0, t1, t2);

                    hit = new(coordinates.T, normal, faceNormal, texturePos, Material);
                    anyHit = true;
                }
            }

            return anyHit;
        }

        public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            if (_vertexBuffer == null || _indexBuffer == null || _primitiveIndices == null || _primitiveIndices.Length == 0)
                return;

            basicEffect.World = _transform;

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
        }
    }
}
