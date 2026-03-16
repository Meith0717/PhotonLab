// SpotLight.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhotonLab.Source.Bodies;
using PhotonLab.Source.Materials;
using Vector3 = System.Numerics.Vector3;

namespace PhotonLab.Source.Lights
{
    internal class SpotLight(
        Microsoft.Xna.Framework.Vector3 position,
        Microsoft.Xna.Framework.Vector3 direction,
        float innerAngleThresholdDeg,
        float outerAngleThresholdDeg,
        Color color,
        float intensity
    ) : LightSource(color, intensity)
    {
        private Microsoft.Xna.Framework.Vector3 _position = position;
        private readonly Vector3 _direction = direction.ToNumerics();

        private readonly float _outerAngleThresholdRad = float.DegreesToRadians(
            outerAngleThresholdDeg
        );
        private readonly float _innerAngleThresholdRad = float.DegreesToRadians(
            innerAngleThresholdDeg
        );

        private readonly Color _color = color;

        protected override Vector3[] GenerateEmittingPositions(MeshBody mesh)
        {
            return [_position.ToNumerics()];
        }

        protected override MeshBody GenerateEmittingMesh(GraphicsDevice graphicsDevice)
        {
            var mesh = BasicBodies.CreateUvSphere(graphicsDevice, 1, 16, 16);
            mesh.SurfaceModel = new GlowingSurfaceModel(NormalMode.Interpolated, _color, intensity);
            mesh.ModelTransform =
                Matrix.CreateScale(.2f)
                * Matrix.CreateTranslation(_position - new Vector3(0, -.2f, 0));
            return mesh;
        }

        protected override float GetAttenuation(Vector3 lightPosition, Vector3 lightDirection)
        {
            var cosAngle = Vector3.Dot(-lightDirection, _direction);
            var cosOuter = float.Cos(_outerAngleThresholdRad);
            var cosInner = float.Cos(_innerAngleThresholdRad);
            var attenuation = (cosAngle - cosOuter) / (cosInner - cosOuter);

            return float.Clamp(attenuation, 0f, 1f);
        }
    }
}
