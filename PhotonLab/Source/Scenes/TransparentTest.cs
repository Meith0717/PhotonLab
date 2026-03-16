// TransparentTest.cs
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

namespace PhotonLab.Source.Scenes
{
    internal class TransparentTest : Scene
    {
        public TransparentTest(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, Color.LightYellow, .002f)
        {
            Camera3D.AddBehaviour(new RotateCamera(.0f, new Vector3(0, 5, 0), 15, 10));
            Camera3D.AddBehaviour(new MoveByMouse(1));
            Camera3D.AddBehaviour(new ZoomByMouse(1));

            LightSources.AddSource(
                new SpotLight(
                    new Vector3(0, 20, 0),
                    new Vector3(0, -1, 0),
                    50,
                    65,
                    Color.LightYellow,
                    1
                )
            );

            CornellBox.Build(graphicsDevice, Meshes, 25, .5f);

            var catModel = ContentProvider.Get<Model>("cat");
            var modelMesh = catModel.Meshes.First();
            var cat = Meshes.AddMesh(new MeshBody(modelMesh, true));
            cat.ModelTransform =
                Matrix.CreateRotationY(float.Pi / 2f) * Matrix.CreateTranslation(0f, 0, 0f);
            cat.SurfaceModel = new TransparentSurfaceModel(NormalMode.Interpolated, 1.55f);
            Meshes.AddMesh(cat);
        }
    }
}
