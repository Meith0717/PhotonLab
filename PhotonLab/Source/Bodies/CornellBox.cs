// CornellBox.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Bodies
{
    internal static class CornellBox
    {
        public static void Build(
            GraphicsDevice graphicsDevice,
            MeshCollection meshes,
            float scale,
            float specularStrength
        )
        {
            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform = Matrix.CreateScale(scale) * Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.SurfaceModel = new PhongModel(
                ContentProvider.Get<Texture2D>("chestBoard10x10"),
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Ceiling
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationX(float.Pi / 2)
                * Matrix.CreateTranslation(0, scale, 0);
            quad.SurfaceModel = new PhongModel(
                Color.Gray,
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Front wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationX(float.Pi)
                * Matrix.CreateTranslation(0, scale / 2, scale / 2);
            quad.SurfaceModel = new PhongModel(
                Color.Blue,
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Back wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) * Matrix.CreateTranslation(0, scale / 2, -scale / 2);
            quad.SurfaceModel = new PhongModel(
                Color.Gray,
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Right wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationY(float.Pi / 2)
                * Matrix.CreateTranslation(-scale / 2, scale / 2, 0);
            quad.SurfaceModel = new PhongModel(
                Color.Green,
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Left wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationY(-float.Pi / 2)
                * Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.SurfaceModel = new PhongModel(Color.Red, NormalMode.Face, 1, specularStrength, 10);
            meshes.AddMesh(quad);
        }

        public static void MirrorBuild(
            GraphicsDevice graphicsDevice,
            MeshCollection meshes,
            float scale,
            float specularStrength,
            float mirrorReflectanceStrength
        )
        {
            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform = Matrix.CreateScale(scale) * Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.SurfaceModel = new PhongModel(
                ContentProvider.Get<Texture2D>("chestBoard10x10"),
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Ceiling
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationX(float.Pi / 2)
                * Matrix.CreateTranslation(0, scale, 0);
            quad.SurfaceModel = new PhongModel(
                Color.Gray,
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Front wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationX(float.Pi)
                * Matrix.CreateTranslation(0, scale / 2, scale / 2);
            quad.SurfaceModel = new PerfectMirrorModel(NormalMode.Face, mirrorReflectanceStrength);
            meshes.AddMesh(quad);

            // Back wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) * Matrix.CreateTranslation(0, scale / 2, -scale / 2);
            quad.SurfaceModel = new PerfectMirrorModel(NormalMode.Face, mirrorReflectanceStrength);
            meshes.AddMesh(quad);

            // Right wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationY(float.Pi / 2)
                * Matrix.CreateTranslation(-scale / 2, scale / 2, 0);
            quad.SurfaceModel = new PhongModel(
                Color.LimeGreen,
                NormalMode.Face,
                1,
                specularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Left wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationY(-float.Pi / 2)
                * Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.SurfaceModel = new PhongModel(Color.Red, NormalMode.Face, 1, specularStrength, 10);
            meshes.AddMesh(quad);
        }
    }
}
