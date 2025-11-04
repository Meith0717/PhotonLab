// MoveByMouse.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoKit.Camera;
using MonoKit.Input;

namespace PhotonLab.scource.Input
{
    public class MoveByMouse() : ICamera3dBehaviour
    {
        private MouseState _prevMouseState;
        private float _yaw;
        private float _pitch;

        public void Initialize(Camera3D owner)
        {
            var x = float.Cos(_pitch) * float.Sin(_yaw);
            var y = float.Sin(_pitch);
            var z = float.Cos(_pitch) * float.Cos(_yaw);
            owner.Forward = new(x, y, z);
            owner.Right = Vector3.Normalize(Vector3.Cross(owner.Forward, Vector3.Up));
            owner.Up = Vector3.Cross(owner.Right, owner.Forward);
        }

        public void Update(Camera3D owner, InputHandler inputHandler, double elapsedGameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            float sensitivity = 0.002f;

            if (inputHandler.HasAction((byte)ActionType.MoveCameraByMouse))
            {
                int deltaX = mouseState.X - _prevMouseState.X;
                int deltaY = mouseState.Y - _prevMouseState.Y;
                _yaw -= deltaX * sensitivity;
                _pitch -= deltaY * sensitivity;

                _pitch = float.Clamp(_pitch, -float.Pi / 2 + 0.01f, float.Pi / 2 - 0.01f);

                var x = float.Cos(_pitch) * float.Sin(_yaw);
                var y = float.Sin(_pitch);
                var z = float.Cos(_pitch) * float.Cos(_yaw);
                owner.Forward = new(x, y, z);
                owner.Right = Vector3.Normalize(Vector3.Cross(owner.Forward, Vector3.Up));
                owner.Up = Vector3.Cross(owner.Right, owner.Forward);
            }

            float movingSpeed = .01f * (float)elapsedGameTime;
            if (ks.IsKeyDown(Keys.W))
                owner.Position += owner.Forward * movingSpeed;
            if (ks.IsKeyDown(Keys.S))
                owner.Position -= owner.Forward * movingSpeed;
            if (ks.IsKeyDown(Keys.A))
                owner.Position -= owner.Right * movingSpeed;
            if (ks.IsKeyDown(Keys.D))
                owner.Position += owner.Right * movingSpeed;
            if (ks.IsKeyDown(Keys.PageUp))
                owner.Position += owner.Up * movingSpeed;
            if (ks.IsKeyDown(Keys.PageDown))
                owner.Position -= owner.Up * movingSpeed;

            _prevMouseState = Mouse.GetState();
        }
    }
}
