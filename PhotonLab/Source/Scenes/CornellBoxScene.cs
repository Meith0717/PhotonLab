// CornellBoxScene.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Input;
using PhotonLab.Source.Materials;

namespace PhotonLab.Source.Scenes
{
    internal class CornellBoxScene : Scene
    {
        private static readonly Vector3[] CamPositions =
        {
            new(0, 12.5f,  30),
            new(30, 12.5f,  0),
            new(0, 12.5f, -30),
            new(-30, 12.5f, 0),
            new(-30, 12.5f, 0),
            new(0, 30f, 0),
        };
        private static readonly Vector3 LookAtPos = new(0, 12.5f, 0);

        private int _cameraPosIndex;

        public CornellBoxScene(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            // Camer3D.AddBehaviour(new RotateCamera(0.01f, LookAtPos, 35, 12.5f));
            Camer3D.AddBehaviour(new MoveByMouse());
            
            CornellBox.Build(graphicsDevice, this, 25);

            var model = BasicBodies.CreateSphere(graphicsDevice, 20, 20);
            model.Material = new MirrorMaterial(Color.White, .75f, NormalMode.Interpolated);
            model.ModelTransform = Matrix.CreateScale(4)
                * Matrix.CreateTranslation(5f, 5f, 5f);
            AddBody(model);

            model = BasicBodies.CreateCube(graphicsDevice, 1, 4f, 1);
            model.Material = new PhongMaterial(Color.Orange, NormalMode.Face);
            model.ModelTransform = Matrix.CreateScale(4)
                * Matrix.CreateTranslation(-9f, 8, 9f);
            AddBody(model);

            model = BasicBodies.CreateSphere(graphicsDevice, 20, 20);
            model.Material = new TransparentMaterial();
            model.ModelTransform = Matrix.CreateScale(4)
                * Matrix.CreateTranslation(-4f, 5f, -8f);
            AddBody(model);
        }

        public override void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            
            inputHandler.DoAction((byte)ActionType.NextCam, () =>
            {
                _cameraPosIndex++;
                _cameraPosIndex %= CamPositions.Length;
                Camer3D.Position = CamPositions[_cameraPosIndex];
                Camer3D.Target = LookAtPos;
            });

            inputHandler.DoAction((byte)ActionType.ResetCam, () =>
            {
                Camer3D.Position = CamPositions[_cameraPosIndex];
                Camer3D.Target = LookAtPos;
            });

            base.Update(elapsedMilliseconds, inputHandler);
        }
    }
}
