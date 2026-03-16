// PlaneScene.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Lights;
using PhotonLab.Source.Materials;
using PhotonLab.Source.Scenes;

internal class PlaneScene : Scene
{
    public PlaneScene(GraphicsDevice graphicsDevice)
        : base(graphicsDevice, Color.LightYellow, .002f)
    {
        Camera3D.AddBehaviour(new RotateCamera(.0f, new Vector3(0, 5, 0), 15, 10));
        Camera3D.AddBehaviour(new MoveByMouse(1));
        Camera3D.AddBehaviour(new ZoomByMouse(1));

        LightSources.AddSource(
            new SpotLight(
                new Vector3(0, 20, 0),
                new Vector3(0, -1, 0),
                55,
                70,
                Color.LightYellow,
                1
            )
        );

        var plane = Meshes.AddMesh(BasicBodies.CreateQuad(graphicsDevice, 100, 100));
        plane.SurfaceModel = new PhongModel(
            ContentProvider.Get<Texture2D>("chestBoard100x100"),
            NormalMode.Face,
            1,
            0,
            0
        );
        plane.ModelTransform = Matrix.CreateScale(2) * Matrix.CreateRotationX(-float.Pi / 2);

        var model = ContentProvider.Get<Model>("dragon");
        var modelMesh = model.Meshes.First();
        var body = Meshes.AddMesh(new MeshBody(modelMesh, true));
        body.ModelTransform =
            Matrix.CreateRotationY(3 * (float.Pi / 4f)) * Matrix.CreateTranslation(0f, 0, 0f);
        body.SurfaceModel = new PerfectMirrorModel(NormalMode.Interpolated, 1);
    }
}
