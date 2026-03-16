// SimpleCornellBoxScene.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Scenes;

internal class SimpleCornellBoxScene : Scene
{
    public SimpleCornellBoxScene(GraphicsDevice graphicsDevice)
        : base(graphicsDevice, Color.LightYellow, .01f)
    {
        Camera3D.AddBehaviour(new MoveByMouse());

        CornellBox.Build(graphicsDevice, Meshes, 25, 0);

        LightSources.AddSource(new PointLight(new Vector3(0, 24.8f, 0), Color.LightYellow, 1));
        // LightSources.AddSource(
        //     new SpotLight(new Vector3(0, 25, 0), new Vector3(0, -1, 0), 40, 80, Color.LightYellow)
        // );

        var model = BasicBodies.CreateCube(graphicsDevice, 1, 4f, 1);
        model.SurfaceModel = new PhongModel(Color.Orange, NormalMode.Face, 1, 1, 10);
        model.ModelTransform = Matrix.CreateScale(4) * Matrix.CreateTranslation(-5f, 8, 4f);
        Meshes.AddMesh(model);

        model = BasicBodies.CreateUvSphere(graphicsDevice, 1, 20, 20);
        model.SurfaceModel = new PhongModel(Color.Violet, NormalMode.Interpolated, 1, 1, 10);
        model.ModelTransform = Matrix.CreateScale(4) * Matrix.CreateTranslation(4f, 5f, 4f);
        Meshes.AddMesh(model);
    }
}
