// CornellBox.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;
using System.Collections.Generic;

namespace PhotonLab.Source.Bodies
{
    internal static class CornellBox
    {
        private static readonly float AmbientStrength = .2f;
        private static readonly float SpecularStrength = .5f;

        public static void Build(GraphicsDevice graphicsDevice, List<MeshBody> bodys, List<ILightSource> lights, float scale)
        { 
            // Floor
            var quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            quad.Material = new PhongMaterial(Color.LightGray) 
            { 
                AmbientStrength = AmbientStrength, 
                SpecularStrength = SpecularStrength 
            };
            bodys.Add(quad);

            // Ceiling
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi / 2) *
                Matrix.CreateTranslation(0, scale, 0);
            quad.Material = new PhongMaterial(Color.LightGray)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            bodys.Add(quad);

            // Front wall
            quad = BasicBodies.CreateQuad(graphicsDevice);
            quad.ModelTransform =
                Matrix.CreateScale(scale) *
                Matrix.CreateRotationX(float.Pi) *
                Matrix.CreateTranslation(0, scale / 2, scale / 2);
            quad.Material = new PhongMaterial(Color.LightGray)
            {
                AmbientStrength = AmbientStrength,
                SpecularStrength = SpecularStrength
            };
            bodys.Add(quad);

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
            bodys.Add(quad);

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
            bodys.Add(quad);

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
            bodys.Add(quad);

            // Light
            lights.Add(new LightSources.SpotLight(new Vector3(0, scale - .1f, 0), new(0, -1, 0), 45, Color.LightYellow));
        }
    }
}
