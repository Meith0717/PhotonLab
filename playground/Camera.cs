using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace playground
{
    public class Camera(GraphicsDevice graphicsDevice)
    {
        public Vector3 Position = new(0, 0, -5); // 5 units back
        public Vector3 Forward;
        public Vector3 Up;
        public Vector3 Right;
        public float Fov = MathF.PI / 3;
        public float AspectRatio = graphicsDevice.Viewport.AspectRatio;

        public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position + Forward, Up);
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(Fov, AspectRatio, .1f, 500);
    }
}
