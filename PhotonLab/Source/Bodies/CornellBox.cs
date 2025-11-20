// CornellBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Graphics.Camera;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;
using PhotonLab.Source.Scenes;

namespace PhotonLab.Source.Bodies
{
    internal static class CornellBox
    {
        private static readonly float AmbientStrength = .1f;
        private static readonly float SpecularStrength = .75f;

        public static void Build(GraphicsDevice graphicsDevice, Scene scene, float scale)
        {
            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.Material = new PhongMaterial(ContentProvider.Get<Texture2D>("chestBoard10x10"))
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Ceiling
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi / 2) *
                Matrix.CreateTranslation(0, scale, 0);
            quad.Material = new PhongMaterial(Color.Gray)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Front wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi) *
                Matrix.CreateTranslation(0, scale / 2, scale / 2);
            quad.Material = new PhongMaterial(Color.Gray)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Back wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateTranslation(0, scale / 2, -scale / 2);
            quad.Material = new PhongMaterial(Color.MediumBlue)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Right wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(float.Pi / 2) *
                Matrix.CreateTranslation(-scale / 2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.LimeGreen)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Left wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(-float.Pi / 2) *
                Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.Red)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Light
            scene.AddLightSource(new LightSources.SpotLight(new Vector3(0, scale - .1f, 0), new(0, -1, 0), 45, Color.LightYellow));
        }
        
        public static void MirrorBuild(GraphicsDevice graphicsDevice, Scene scene, float scale, float mirrorStrength)
        {
            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.Material = new PhongMaterial(ContentProvider.Get<Texture2D>("chestBoard10x10"))
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Ceiling
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi / 2) *
                Matrix.CreateTranslation(0, scale, 0);
            quad.Material = new PhongMaterial(Color.Gray)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Front wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi) *
                Matrix.CreateTranslation(0, scale / 2, scale / 2);
            quad.Material = new MirrorMaterial(Color.White, mirrorStrength, NormalMode.Face);
            scene.AddBody(quad);

            // Back wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateTranslation(0, scale / 2, -scale / 2);
            quad.Material = new MirrorMaterial(Color.White, mirrorStrength, NormalMode.Face);
            scene.AddBody(quad);

            // Right wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(float.Pi / 2) *
                Matrix.CreateTranslation(-scale / 2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.LimeGreen)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Left wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationY(-float.Pi / 2) *
                Matrix.CreateTranslation(scale / 2, scale / 2, 0);
            quad.Material = new PhongMaterial(Color.Red)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            scene.AddBody(quad);

            // Light
            scene.AddLightSource(new LightSources.SpotLight(new Vector3(0, scale - .1f, 0), new(0, -1, 0), 45, Color.LightYellow));
        }
    }
}
