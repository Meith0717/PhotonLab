// LightSources.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Generic;
using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal static class LightSources
    {
        internal class PointLight(
            Microsoft.Xna.Framework.Vector3 position,
            Microsoft.Xna.Framework.Color color
        ) : LightSource(color)
        {
            private Microsoft.Xna.Framework.Vector3 _position = position;

            protected override Vector3[] GenerateEmittingPositions()
            {
                return [_position.ToNumerics()];
            }

            protected override float GetAttenuation(
                Vector3 lightPosition,
                Vector3 lightDirection
            ) => 1;
        }

        internal class SpotLight(
            Microsoft.Xna.Framework.Vector3 position,
            Microsoft.Xna.Framework.Vector3 direction,
            float innerAngleThresholdDeg,
            float outerAngleThresholdDeg,
            Microsoft.Xna.Framework.Color color
        ) : LightSource(color)
        {
            private Microsoft.Xna.Framework.Vector3 _position = position;
            private readonly Vector3 _direction = direction.ToNumerics();

            private readonly float _outerAngleThresholdRad = float.DegreesToRadians(
                outerAngleThresholdDeg
            );
            private readonly float _innerAngleThresholdRad = float.DegreesToRadians(
                innerAngleThresholdDeg
            );

            protected override Vector3[] GenerateEmittingPositions()
            {
                return [_position.ToNumerics()];
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
}
