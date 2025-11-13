// Scene.cs 
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
        private readonly static Vector3[] _camPositions =
        {
            new(0, 12.5f,  30),
            new(30, 12.5f,  0),
            new(0, 12.5f, -30),
            new(-30, 12.5f, 0),
            new(-30, 12.5f, 0),
            new(0, 30f, 0),
        };
        public readonly static Vector3 _lookAtPos = new(0, 12.5f, 0);

        private readonly MeshBody _rotatingBody;
        private float _rotaton;
        private int _cameraPosIndex;


        public CornellBoxScene(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Camer3D.AddBehaviour(new MoveByMouse());
            Camer3D.AddBehaviour(new ZoomByMouse(1));

            CornellBox.Build(graphicsDevice, this, 25);

            var model = BasicBodies.CreateSphere(graphicsDevice, 30, 30);
            model.Material = new MirrorMaterial(Color.White, .75f);
            model.ModelTransform = Matrix.CreateScale(4)
                * Matrix.CreateTranslation(5f, 5f, 5f);
            AddBody(_rotatingBody = model);

            model = BasicBodies.CreateCube(graphicsDevice, 1, 4f, 1);
            model.Material = new PhongMaterial(Color.Orange, NormalMode.Face);
            model.ModelTransform = Matrix.CreateScale(4)
                * Matrix.CreateTranslation(-9f, 8, 9f);
            AddBody(_rotatingBody = model);

            model = BasicBodies.CreateSphere(graphicsDevice, 30, 30);
            model.Material = new TransparentMaterial();
            model.ModelTransform = Matrix.CreateScale(4)
                * Matrix.CreateTranslation(-4f, 5f, -8f);
            AddBody(model);
        }

        public override void Update(double elapsedMilliseconds, InputHandler inputHandler)
        {
            _rotaton += .05f;

            inputHandler.DoAction((byte)ActionType.NextCam, () =>
            {
                _cameraPosIndex++;
                _cameraPosIndex %= _camPositions.Length;
                Camer3D.Position = _camPositions[_cameraPosIndex];
                Camer3D.Target = _lookAtPos;
            });

            inputHandler.DoAction((byte)ActionType.ResetCam, () =>
            {
                Camer3D.Position = _camPositions[_cameraPosIndex];
                Camer3D.Target = _lookAtPos;
            });

            base.Update(elapsedMilliseconds, inputHandler );
        }
    }
}
