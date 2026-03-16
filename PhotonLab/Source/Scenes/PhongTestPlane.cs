// PhongTestPlane.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Scenes;

internal class PhongTestPlane : Scene
{
    public PhongTestPlane(GraphicsDevice graphicsDevice)
        : base(graphicsDevice, Color.Gray, .5f)
    {
        Camera3D.AddBehaviour(new RotateCamera(.0f, new Vector3(0, 1.5f, 0), 8, 7));
        Camera3D.AddBehaviour(new MoveByMouse(1));
        Camera3D.AddBehaviour(new ZoomByMouse(1));

        LightSources.AddSource(
            new SpotLight(
                new Vector3(0, 25, 0),
                new Vector3(0, -1, 0),
                40,
                80,
                Color.LightYellow,
                1
            )
        );

        // LightSources.AddSource(
        //     new QuadLight(
        //         Matrix.CreateRotationX(float.Pi / 2) * Matrix.CreateTranslation(0, 20, 0),
        //         Color.LightYellow,
        //         10,
        //         10,
        //         1
        //     )
        // );

        // LightSources.AddSource(
        //     new HemisphereLight(
        //         Matrix.CreateRotationX(float.Pi) * Matrix.CreateTranslation(0, 20, 0),
        //         Color.LightYellow,
        //         1,
        //         16,
        //         8
        //     )
        // );

        var model = BasicBodies.CreateQuad(graphicsDevice);
        model.ModelTransform = Matrix.CreateScale(200) * Matrix.CreateRotationX(-float.Pi / 2);
        model.SurfaceModel = new PhongModel(Color.White, NormalMode.Face, 1, 0, 0);
        Meshes.AddMesh(model);

        model = BasicBodies.CreateUvSphere(graphicsDevice, 1, 32, 32);
        model.ModelTransform = Matrix.CreateScale(3) * Matrix.CreateTranslation(0, 3.1f, 0);
        model.SurfaceModel = new PhongModel(Color.Red, NormalMode.Interpolated, 1, 1, 10);
        Meshes.AddMesh(model);
    }
}
