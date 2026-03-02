// PlaneScene.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Input;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Scenes
{
    internal class PlaneScene : Scene
    {
        public PlaneScene(GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            Camera3D.AddBehaviour(new RotateCamera(.0f, new Vector3(0, 5, 0), 15, 10));
            Camera3D.AddBehaviour(new MoveByMouse(1));
            Camera3D.AddBehaviour(new ZoomByMouse(1));

            AddLightSource(new LightSources.PointLight(new Vector3(0, 20, 0), Color.LightYellow));

            var model = BasicBodies.CreateQuad(graphicsDevice);
            model.ModelTransform = Matrix.CreateScale(100) * Matrix.CreateRotationX(-float.Pi / 2); // floor rotation
            model.Material = new PhongMaterial(
                ContentProvider.Get<Texture2D>("chestBoard100x100"),
                NormalMode.Face
            )
            {
                SpecularStrength = 0,
                AmbientStrength = 0,
            };
            Meshes.AddMesh(model);

            model = BasicBodies.CreateSphere(graphicsDevice, 30, 30);
            model.ModelTransform = Matrix.CreateScale(4) * Matrix.CreateTranslation(0, 5, 0);
            model.Material = new PhongMaterial(Color.Red, NormalMode.Interpolated)
            {
                SpecularStrength = 1,
                AmbientStrength = 0,
            };
            Meshes.AddMesh(model);
        }
    }
}
