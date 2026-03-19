// CornellBoxScene.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Linq;
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
    internal class CornellBoxScene : Scene
    {
        private static readonly Vector3[] CamPositions =
        {
            new(0, 12.5f, 34),
            new(34, 12.5f, 0),
            new(0, 12.5f, -34),
            new(-34, 12.5f, 0),
            new(-34, 12.5f, 0),
            new(0, 34, 0),
        };
        private static readonly Vector3 LookAtPos = new(0, 12.5f, 0);

        private int _cameraPosIndex;

        public CornellBoxScene(GraphicsDevice graphicsDevice)
            : base(graphicsDevice, Color.LightYellow, .0f)
        {
            //Camera3D.AddBehaviour(new RotateCamera(0.015f, LookAtPos, 35, 12.5f));
            Camera3D.AddBehaviour(new MoveByMouse());

            CornellBox.Build(graphicsDevice, Meshes, 25, 1);

            var segments = 16;

            LightSources.AddSource(
                new HemisphereLight(
                    Matrix.CreateRotationX(float.Pi) * Matrix.CreateTranslation(0, 25.2f, 10),
                    Color.Red,
                    10,
                    segments,
                    segments / 2
                )
            );

            LightSources.AddSource(
                new HemisphereLight(
                    Matrix.CreateRotationX(float.Pi) * Matrix.CreateTranslation(8.66f, 25.2f, -5),
                    Color.Green,
                    10,
                    segments,
                    segments / 2
                )
            );

            LightSources.AddSource(
                new HemisphereLight(
                    Matrix.CreateRotationX(float.Pi) * Matrix.CreateTranslation(-8.66f, 25.2f, -5),
                    Color.Blue,
                    10,
                    segments,
                    segments / 2
                )
            );

            LightSources.AddSource(
                new HemisphereLight(
                    Matrix.CreateRotationX(float.Pi) * Matrix.CreateTranslation(0, 25.2f, 0),
                    Color.LightYellow,
                    2,
                    segments,
                    segments / 2
                )
            );

            var mirrorSphere = Meshes.AddMesh(
                BasicBodies.CreateUvSphere(graphicsDevice, 4, 20, 20)
            );
            mirrorSphere.SurfaceModel = new PerfectMirrorModel(NormalMode.Interpolated, 1);
            mirrorSphere.ModelTransform = Matrix.CreateTranslation(0f, 4.1f, 8f);

            var highCube = Meshes.AddMesh(BasicBodies.CreateCube(graphicsDevice, 5, 10f, 5));
            highCube.SurfaceModel = new PhongModel(Color.MonoGameOrange, NormalMode.Face, 1, 1, 10);
            highCube.ModelTransform = Matrix.CreateTranslation(9f, 5, 0f);

            var tetrahedron = Meshes.AddMesh(BasicBodies.CreateTetrahedron(graphicsDevice, 6));
            tetrahedron.SurfaceModel = new PhongModel(Color.Red, NormalMode.Face, 1, 1, 10);
            tetrahedron.ModelTransform =
                Matrix.CreateRotationY(float.Pi) * Matrix.CreateTranslation(-9f, 0, 0f);

            var catModel = ContentProvider.Get<Model>("cat");
            var modelMesh = catModel.Meshes.First();

            var cat = Meshes.AddMesh(new MeshBody(modelMesh, true));
            cat.SurfaceModel = new TransparentSurfaceModel(NormalMode.Interpolated, 1.5f);
            cat.ModelTransform =
                Matrix.CreateRotationY(float.Pi / 2f) * Matrix.CreateTranslation(0f, 0, -8f);

            var smallSphere = Meshes.AddMesh(BasicBodies.CreateUvSphere(graphicsDevice, 2, 20, 20));
            smallSphere.SurfaceModel = new PhongModel(Color.Olive, NormalMode.Face, 1, 1, 10);
            smallSphere.ModelTransform = Matrix.CreateTranslation(-8f, 2.1f, -8f);
        }

        public override void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            inputHandler.DoAction(
                (byte)ActionType.NextCam,
                () =>
                {
                    _cameraPosIndex++;
                    _cameraPosIndex %= CamPositions.Length;
                    Camera3D.Position = CamPositions[_cameraPosIndex];
                    Camera3D.Target = LookAtPos;
                }
            );

            inputHandler.DoAction(
                (byte)ActionType.ResetCam,
                () =>
                {
                    Camera3D.Position = CamPositions[_cameraPosIndex];
                    Camera3D.Target = LookAtPos;
                }
            );

            base.Update(elapsedMilliseconds, inputHandler);
        }
    }
}
