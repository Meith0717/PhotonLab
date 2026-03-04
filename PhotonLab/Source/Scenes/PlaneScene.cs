// PlaneScene.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Scenes
{
    internal class PlaneScene : Scene
    {
        public PlaneScene(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, Color.LightYellow, .002f)
        {
            Camera3D.AddBehaviour(new RotateCamera(.0f, new Vector3(0, 5, 0), 15, 10));
            Camera3D.AddBehaviour(new MoveByMouse(1));
            Camera3D.AddBehaviour(new ZoomByMouse(1));

            LightSources.AddSource(
                new LightSources.SpotLight(
                    new Vector3(0, 20, 0),
                    new Vector3(0, -1, 0),
                    30,
                    50,
                    Color.LightYellow
                )
            );

            var model = BasicBodies.CreateQuad(graphicsDevice);
            model.ModelTransform = Matrix.CreateScale(100) * Matrix.CreateRotationX(-float.Pi / 2);
            model.SurfaceModel = new PhongModel(
                ContentProvider.Get<Texture2D>("chestBoard100x100"),
                NormalMode.Face,
                1,
                0,
                0
            );
            Meshes.AddMesh(model);

            model = BasicBodies.CreateSphere(graphicsDevice, 30, 30);
            model.ModelTransform = Matrix.CreateScale(4) * Matrix.CreateTranslation(0, 5, 0);
            model.SurfaceModel = new PerfectMirrorModel(NormalMode.Interpolated);
            Meshes.AddMesh(model);
        }
    }
}
