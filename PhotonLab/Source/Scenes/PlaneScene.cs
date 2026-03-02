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
            Camer3D.AddBehaviour(new RotateCamera(.0f, new Vector3(0, 0, 0), 30, 30));

            AddLightSource(
                new LightSources.SpotLight(
                    new Vector3(0, 20, 0),
                    new Vector3(0, -1, 0),
                    40,
                    40,
                    Color.LightYellow
                )
            );

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
            AddBody(model);

            //model = BasicBodies.CreateSphere(graphicsDevice, 30, 30);
            //model.Material = new TransparentMaterial();
            //model.ModelTransform = Matrix.CreateScale(4) * Matrix.CreateTranslation(0, 5, 0);
            //AddBody(model);
        }
    }
}
