// Screen.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Camera;
using MonoKit.Input;
using PhotonLab.Input;

namespace PhotonLab
{
    internal class Screen
    {
        private Camera3D _camera3D;
        private Camera3D _camera3D1;

        private VertexBuffer _triangleVertices;
        private BasicEffect _basicEffect;
        private RayTracer _rayTracer;

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _camera3D = new(graphicsDevice);
            _camera3D1 = new(graphicsDevice);
            _camera3D.AddBehaviour(new MoveByMouse());
            _camera3D1.AddBehaviour(new ZoomByMouse(1));
            _basicEffect = new BasicEffect(graphicsDevice);
            _rayTracer = new(graphicsDevice);

            // Initialize triangle
            _triangleVertices = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), 4, BufferUsage.None);
            _triangleVertices.SetData(new VertexPositionColor[3]
            {
                new(new(0, 1, 5), Color.Black),
                new(new(-1, -1, 5), Color.Black),
                new(new(1, -1, 5), Color.Black)
            });
        }

        public void Update(double elapsedMolloseconds, InputHandler inputHandler)
        {
            _camera3D.Update(elapsedMolloseconds, inputHandler);
            _camera3D1.Update(elapsedMolloseconds, inputHandler);
            if (inputHandler.HasAction((byte)ActionType.RayTrace))
            {
                _rayTracer.Initialize(_camera3D);

                var vertecies = new VertexPositionColor[3];
                _triangleVertices.GetData(vertecies);
                _rayTracer.ShadeIntersectedCameraRays((vertecies[0].Position, vertecies[1].Position, vertecies[2].Position), Matrix.Invert(_camera3D.View));
                _rayTracer.SaveImageFromColor();
            }
        }

        public void Draw(double elapsedMolloseconds, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) 
        {
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = _camera3D.View;
            _basicEffect.Projection = _camera3D.Projection;

            graphicsDevice.SetVertexBuffer(_triangleVertices);
            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }

            if (_rayTracer.VertexBuffer is not null)
            {
                graphicsDevice.SetVertexBuffer(_rayTracer.VertexBuffer);
                foreach (var pass in _basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, _rayTracer.VertexBuffer.VertexCount);
                }
            }

            Camera3DGizmo.Draw(graphicsDevice, _camera3D, _camera3D1);
        }
    }
}
