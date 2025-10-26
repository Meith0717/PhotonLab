// Sceen.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using MonoKit.Core;
using MonoKit.Input;
using PhotonLab.Input;

namespace PhotonLab
{
    public enum Paths { Images }

    internal class Sceen
    {
        private readonly Camera3D _camera3D;
        private readonly BasicEffect _basicEffect;
        private readonly RayTracer _rayTracer;
        private readonly Shape3D _3dObject;

        public Sceen(GraphicsDevice graphicsDevice)
        {
            _camera3D = new(graphicsDevice);
            _camera3D.AddBehaviour(new MoveByMouse());
            _camera3D.AddBehaviour(new ZoomByMouse(1));
            _basicEffect = new(graphicsDevice);
            _rayTracer = new(graphicsDevice);
            _3dObject = Shape3D.CreateTetrahedron(graphicsDevice);
        }

        public void Update(double elapsedMolloseconds, InputHandler inputHandler, PathManager<Paths> pathManager)
        {
            _3dObject.ModelTransform = Matrix.CreateScale(1) * Matrix.CreateTranslation(0, 0, 5);
            _camera3D.Update(elapsedMolloseconds, inputHandler);
            if (inputHandler.HasAction((byte)ActionType.RayTrace))
            {
                _rayTracer.Initialize(_camera3D);

                var vertecies = _3dObject.GetVertecies();
                _rayTracer.ShadeIntersectedCameraRays(
                    (vertecies[0].Position, vertecies[1].Position, vertecies[2].Position),
                    (vertecies[0].Color.ToVector3(), vertecies[1].Color.ToVector3(), vertecies[2].Color.ToVector3()),
                    Matrix.Invert(_camera3D.View));
                _rayTracer.SaveImageFromColor(pathManager);
            }
        }

        public void Draw(double elapsedMolloseconds, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = _camera3D.View;
            _basicEffect.Projection = _camera3D.Projection;
            _basicEffect.VertexColorEnabled = true;

            _3dObject.Draw(graphicsDevice, _basicEffect);
        }
    }
}
