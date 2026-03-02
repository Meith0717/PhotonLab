// MeshBody.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Immutable;
using System.Linq;

namespace PhotonLab.Source.Bodies
{
    using System;
    using System.Numerics;
    using System.Threading.Tasks;
    using Microsoft.Xna.Framework.Graphics;
    using PhotonLab.Source.Materials;
    using PhotonLab.Source.RayTracing;

    internal class MeshBody : IBody3D
    {
        // CPU stuff (Ray Tracing)
        public Matrix4x4 InvTransform { get; private set; }
        public readonly BoundingBoxSIMD BoundingBox;
        public readonly ushort[] PrimitiveIndices;
        public readonly Vector3[] VertexPositions;
        public readonly Vector3[] VertexNormals;
        public readonly Vector2[] VertexTextures;
        private Matrix4x4 _transform;

        // GPU stuff (Rasterisation)
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        // Some other Stuff
        public int FacesCount => PrimitiveIndices.Length / 3;
        public IMaterial Material { get; set; }
        public Microsoft.Xna.Framework.Matrix ModelTransform
        {
            set
            {
                _transform = value.ToNumerics();
                Matrix4x4.Invert(_transform, out var invTransform);
                InvTransform = invTransform;
            }
            get => _transform;
        }

        public MeshBody(
            GraphicsDevice graphicsDevice,
            VertexPositionNormalTexture[] vertices,
            ushort[] indices
        )
        {
            var vertexCount = vertices.Length;
            VertexPositions = new Vector3[vertexCount];
            VertexNormals = new Vector3[vertexCount];
            VertexTextures = new Vector2[vertexCount];
            Parallel.For(
                0,
                vertexCount,
                i =>
                {
                    VertexPositions[i] = vertices[i].Position.ToNumerics();
                    VertexNormals[i] = vertices[i].Normal.ToNumerics();
                    VertexTextures[i] = vertices[i].TextureCoordinate.ToNumerics();
                }
            );

            PrimitiveIndices = indices;
            BoundingBox = BoundingBoxSIMD.CreateFromPoints(VertexPositions);

            _vertexBuffer = new VertexBuffer(
                graphicsDevice,
                typeof(VertexPositionNormalTexture),
                vertices.Length,
                BufferUsage.None
            );
            _vertexBuffer.SetData(vertices);
            _indexBuffer = new IndexBuffer(
                graphicsDevice,
                IndexElementSize.SixteenBits,
                PrimitiveIndices.Length,
                BufferUsage.None
            );
            _indexBuffer.SetData(PrimitiveIndices);
        }

        public MeshBody(ModelMesh mesh)
        {
            var mainMesh = mesh.MeshParts[0];
            _indexBuffer = mainMesh.IndexBuffer;
            _vertexBuffer = mainMesh.VertexBuffer;

            if (mesh.MeshParts.Any(part => mainMesh.VertexBuffer != part.VertexBuffer))
                throw new Exception();

            var vertexCount = mainMesh.NumVertices;
            var primitiveCount = mainMesh.PrimitiveCount;

            // Load index data from GPU
            PrimitiveIndices = new ushort[primitiveCount * 3];
            _indexBuffer.GetData(PrimitiveIndices);

            // Load vertex data from GPU
            VertexPositions = new Vector3[vertexCount];
            VertexNormals = new Vector3[vertexCount];
            VertexTextures = new Vector2[vertexCount];
            var vertexBufferData = new VertexPositionNormalTexture[vertexCount];
            _vertexBuffer.GetData(vertexBufferData);
            Parallel.For(
                0,
                vertexCount,
                i =>
                {
                    VertexPositions[i] = vertexBufferData[i].Position.ToNumerics();
                    VertexNormals[i] = vertexBufferData[i].Normal.ToNumerics();
                    VertexTextures[i] = vertexBufferData[i].TextureCoordinate.ToNumerics();
                }
            );

            BoundingBox = BoundingBoxSIMD.CreateFromPoints(VertexNormals);
        }

        public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
        {
            if (
                _vertexBuffer == null
                || _indexBuffer == null
                || PrimitiveIndices == null
                || PrimitiveIndices.Length == 0
            )
                return;

            basicEffect.World = _transform;

            // Draw main mesh
            basicEffect.VertexColorEnabled = basicEffect.TextureEnabled = false;
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
                graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    PrimitiveIndices.Length / 3
                );
            }
        }
    }
}
