// CornellMirror.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Scenes;

internal class CornellMirrorScene : Scene
{
    public CornellMirrorScene(GraphicsDevice graphicsDevice)
        : base(graphicsDevice, Color.White, .02f)
    {
        Camera3D.AddBehaviour(new MoveByMouse(5));

        CornellBox.MirrorBuild(graphicsDevice, Meshes, 25, 0f, .98f);

        LightSources.AddSource(
            new SpotLight(
                new Vector3(0, 24.5f, 0),
                new Vector3(0, -1, 0),
                65,
                80,
                Color.LightYellow,
                1
            )
        );

        var model = BasicBodies.CreateCube(graphicsDevice, 1, 4f, 1);
        model.SurfaceModel = new PhongModel(Color.MonoGameOrange, NormalMode.Face, 1, 1, 10);
        model.ModelTransform = Matrix.CreateScale(4) * Matrix.CreateTranslation(-9f, 8, 4f);
        Meshes.AddMesh(model);

        model = BasicBodies.CreateUvSphere(graphicsDevice, 1, 20, 20);
        model.SurfaceModel = new PhongModel(Color.DeepSkyBlue, NormalMode.Interpolated, 1, 1, 10);
        model.ModelTransform = Matrix.CreateScale(4) * Matrix.CreateTranslation(8f, 5f, -3f);
        Meshes.AddMesh(model);
    }
}
