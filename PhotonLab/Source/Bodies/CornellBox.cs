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
        public static float SpecularStrength { get; set; } = 1f;

        public static void Build(
            GraphicsDevice graphicsDevice,
            MeshCollection meshes,
            LightSourceCollection lightSources,
            float scale
        )
        {
            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform = Matrix.CreateScale(scale) * Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.SurfaceModel = new PhongModel(
                ContentProvider.Get<Texture2D>("chestBoard10x10"),
                NormalMode.Face,
                1,
                SpecularStrength,
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
                SpecularStrength,
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
                Color.Gray,
                NormalMode.Face,
                1,
                SpecularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Back wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) * Matrix.CreateTranslation(0, scale / 2, -scale / 2);
            quad.SurfaceModel = new PhongModel(
                Color.MediumBlue,
                NormalMode.Face,
                1,
                SpecularStrength,
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
                Color.LimeGreen,
                NormalMode.Face,
                1,
                SpecularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Left wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationY(-float.Pi / 2)
                * Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.SurfaceModel = new PhongModel(Color.Red, NormalMode.Face, 1, SpecularStrength, 10);
            meshes.AddMesh(quad);

            // Light
            lightSources.AddSource(
                new LightSources.SpotLight(
                    new Vector3(0, scale - .1f, 0),
                    new Vector3(0, -1, 0),
                    50,
                    60,
                    Color.LightYellow
                )
            );
        }

        public static void MirrorBuild(
            GraphicsDevice graphicsDevice,
            MeshCollection meshes,
            LightSourceCollection lightSources,
            float scale,
            float mirrorStrength
        )
        {
            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform = Matrix.CreateScale(scale) * Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.SurfaceModel = new PhongModel(
                ContentProvider.Get<Texture2D>("chestBoard10x10"),
                NormalMode.Face,
                1,
                SpecularStrength,
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
                SpecularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Front wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationX(float.Pi)
                * Matrix.CreateTranslation(0, scale / 2, scale / 2);
            quad.SurfaceModel = new PerfectMirrorModel(NormalMode.Face);
            meshes.AddMesh(quad);

            // Back wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) * Matrix.CreateTranslation(0, scale / 2, -scale / 2);
            quad.SurfaceModel = new PerfectMirrorModel(NormalMode.Face);
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
                SpecularStrength,
                10
            );
            meshes.AddMesh(quad);

            // Left wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale)
                * Matrix.CreateRotationY(-float.Pi / 2)
                * Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.SurfaceModel = new PhongModel(Color.Red, NormalMode.Face, 1, SpecularStrength, 10);
            meshes.AddMesh(quad);

            // Light
            lightSources.AddSource(
                new LightSources.SpotLight(
                    new Vector3(0, scale - .1f, 0),
                    new Vector3(0, -1, 0),
                    45,
                    50,
                    Color.LightYellow
                )
            );
        }
    }
}
