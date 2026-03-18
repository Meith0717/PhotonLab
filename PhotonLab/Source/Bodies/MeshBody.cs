// MeshBody.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Materials;
using PhotonLab.Source.RayTracing;
using PhotonLab.Source.RayTracing.Data;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Bodies;

internal class MeshBody : IBody3D
{
    private readonly IndexBuffer _indexBuffer;

    // GPU stuff (Rasterisation)
    private readonly VertexBuffer _vertexBuffer;
    public readonly BoundingBoxSIMD BoundingBox;
    public readonly ushort[] PrimitiveIndices;
    public readonly Vector3[] VertexNormals;
    public readonly Vector3[] VertexPositions;
    public readonly Vector2[] VertexTextures;
    private Matrix4x4 _transform;

    public MeshBody(
        GraphicsDevice graphicsDevice,
        VertexPositionNormalTexture[] vertices,
        ushort[] primitiveIndices
    )
    {
        PrimitiveIndices = primitiveIndices;

        ExtractVerticesData(vertices, out VertexPositions, out VertexNormals, out VertexTextures);

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

    public MeshBody(ModelMesh mesh, bool fixMesh)
    {
        var mainMesh = mesh.MeshParts[0];
        if (mesh.MeshParts.Any(part => mainMesh.VertexBuffer != part.VertexBuffer))
            throw new Exception();

        // Load index data from GPU
        PrimitiveIndices = new ushort[mainMesh.PrimitiveCount * 3];
        _indexBuffer = mainMesh.IndexBuffer;
        _indexBuffer.GetData(PrimitiveIndices);

        if (fixMesh)
        {
            for (var i = 0; i < PrimitiveIndices.Length; i += 3)
            {
                (PrimitiveIndices[i + 1], PrimitiveIndices[i + 2]) = (
                    PrimitiveIndices[i + 2],
                    PrimitiveIndices[i + 1]
                );
            }
            _indexBuffer.SetData(PrimitiveIndices);
        }

        // Load vertex data from GPU
        var vertices = new VertexPositionNormalTexture[mainMesh.NumVertices];
        _vertexBuffer = mainMesh.VertexBuffer;
        _vertexBuffer.GetData(vertices);
        ExtractVerticesData(vertices, out VertexPositions, out _, out VertexTextures);
        CalculateNormals(PrimitiveIndices, VertexPositions, out VertexNormals);

        BoundingBox = BoundingBoxSIMD.CreateFromPoints(VertexPositions);
    }

    public Matrix4x4 InvTransform { get; private set; }
    public int FacesCount => PrimitiveIndices.Length / 3;
    public ISurfaceModel SurfaceModel { get; set; }

    public Matrix ModelTransform
    {
        set
        {
            _transform = value.ToNumerics();
            Matrix4x4.Invert(_transform, out var invTransform);
            InvTransform = invTransform;
        }
        get => _transform;
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
        if (SurfaceModel is not null)
        {
            switch (SurfaceModel)
            {
                case ITexturedSurface { Texture: not null } texturedSurface:
                    basicEffect.Texture = texturedSurface.Texture.Texture2D;
                    basicEffect.TextureEnabled = true;
                    break;
                case IColoredSurface coloredSurface:
                    basicEffect.DiffuseColor = coloredSurface.Color.ToVector3();
                    break;
                default:
                    basicEffect.DiffuseColor = Vector3.One;
                    break;
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

    private void ExtractVerticesData(
        VertexPositionNormalTexture[] vertices,
        out Vector3[] vertexPositions,
        out Vector3[] vertexNormals,
        out Vector2[] vertexTextures
    )
    {
        vertexPositions = new Vector3[vertices.Length];
        vertexTextures = new Vector2[vertices.Length];
        vertexNormals = new Vector3[vertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            vertexPositions[i] = vertices[i].Position.ToNumerics();
            vertexNormals[i] = vertices[i].Normal.ToNumerics();
            vertexTextures[i] = vertices[i].TextureCoordinate.ToNumerics();
        }
    }

    private void CalculateNormals(
        ushort[] primitiveIndices,
        Vector3[] vertexPositions,
        out Vector3[] vertexNormals
    )
    {
        vertexNormals = new Vector3[vertexPositions.Length];

        var positionToIndices = new Dictionary<Vector3, List<int>>();
        for (var i = 0; i < vertexPositions.Length; i++)
        {
            if (!positionToIndices.TryGetValue(vertexPositions[i], out var list))
            {
                list = [];
                positionToIndices[vertexPositions[i]] = list;
            }
            list.Add(i);
        }

        for (var i = 0; i < primitiveIndices.Length; i += 3)
        {
            var i0 = primitiveIndices[i];
            var i1 = primitiveIndices[i + 1];
            var i2 = primitiveIndices[i + 2];

            var p0 = vertexPositions[i0];
            var p1 = vertexPositions[i1];
            var p2 = vertexPositions[i2];

            var faceNormal = Vector3.Cross(p1 - p0, p2 - p0);

            vertexNormals[i0] += faceNormal;
            vertexNormals[i1] += faceNormal;
            vertexNormals[i2] += faceNormal;
        }

        foreach (var entry in positionToIndices)
        {
            var sharedIndices = entry.Value;
            var summedNormal = Vector3.Zero;

            foreach (var idx in sharedIndices)
                summedNormal += vertexNormals[idx];

            summedNormal =
                summedNormal.LengthSquared() > 0 ? Vector3.Normalize(summedNormal) : Vector3.UnitY;

            foreach (var idx in sharedIndices)
                vertexNormals[idx] = summedNormal;
        }
    }
}
