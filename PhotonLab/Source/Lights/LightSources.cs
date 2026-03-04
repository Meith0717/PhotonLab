// LightSources.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Collections.Generic;
using System.Numerics;
using PhotonLab.Source.RayTracing;

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
                var res = new List<Vector3>();
                for (var i = -0; i < 1; i++)
                for (var j = -0; j < 1; j++)
                    res.Add(
                        new Vector3(_position.X + (i * .3f), _position.Y, _position.Z + (j * .3f))
                    );
                return res.ToArray();
            }

            protected override float GetAttenuation(
                Vector3 lightPosition,
                in SurfaceIntersectionData surfaceIntersectionData
            )
            {
                return 1;
            }
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

            protected override float GetAttenuation(
                Vector3 lightPosition,
                in SurfaceIntersectionData surfaceIntersectionData
            )
            {
                var toLight = lightPosition - surfaceIntersectionData.Position;
                var toLightDir = Vector3.Normalize(toLight);
                var angle = float.Acos(Vector3.Dot(-toLightDir, _direction));

                // Calculate smooth angular attenuation
                var cosAngle = float.Cos(angle);
                var cosOuter = float.Cos(_outerAngleThresholdRad);
                var cosInner = float.Cos(_innerAngleThresholdRad);
                return float.Clamp((cosAngle - cosOuter) / (cosInner - cosOuter), 0f, 1f);
            }
        }
    }
}
