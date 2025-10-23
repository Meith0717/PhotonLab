using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Camera;
using System;

namespace PhotonLab
{
    public static class Camera3DGizmo
    {
        public static void Draw(GraphicsDevice graphicsDevice, Camera3D mainCamera, Camera3D cam)
        {
            Vector3 camPos = cam.Position;
            Vector3 forward = cam.Forward * 10;
            Vector3 up = cam.Up * 10;
            Vector3 right = cam.Right * 10;

            float near = cam.NearPlane;
            float aspect = cam.AspectRatio;
            float fov = cam.Fov;

            // Compute near/far plane sizes
            float nearHeight = 2f * near * MathF.Tan(fov / 2f);
            float nearWidth = nearHeight * aspect;

            // Plane centers
            Vector3 nearCenter = camPos + forward * near;

            // Corners
            Vector3 nTL = nearCenter + up * nearHeight / 2 - right * nearWidth / 2;
            Vector3 nTR = nearCenter + up * nearHeight / 2 + right * nearWidth / 2;
            Vector3 nBL = nearCenter - up * nearHeight / 2 - right * nearWidth / 2;
            Vector3 nBR = nearCenter - up * nearHeight / 2 + right * nearWidth / 2;

            // Build lines (LineList)
            VertexPositionColor[] lines = new VertexPositionColor[]
            {
                // From camera to near plane (red)
                new(camPos, Color.White), new(nTL, Color.White),
                new(camPos, Color.White), new(nTR, Color.White),
                new(camPos, Color.White), new(nBL, Color.White),
                new(camPos, Color.White), new(nBR, Color.White),

                // Near plane rectangle (green)
                new(nTL, Color.White), new(nTR, Color.White),
                new(nTR, Color.White), new(nBR, Color.White),
                new(nBR, Color.White), new(nBL, Color.White),
                new(nBL, Color.White), new(nTL, Color.White),
            };

            BasicEffect effect = new(graphicsDevice)
            {
                VertexColorEnabled = true,
                View = mainCamera.View,
                Projection = mainCamera.Projection,
                World = Matrix.Identity
            };

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.LineList,
                    lines, 0, lines.Length / 2
                );
            }
        }
    }
}
