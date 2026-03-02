// LightSources.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.ComponentModel.Design.Serialization;
using System.Numerics;

namespace PhotonLab.Source.Lights
{
    internal static class LightSources
    {
        private static Vector3[] _emissionPoints => [Vector3.Zero];

        internal class PointLight(
            Microsoft.Xna.Framework.Vector3 position,
            Microsoft.Xna.Framework.Color color
        ) : ILightSource
        {
            public Vector3 Position => _position.ToNumerics();
            public Vector3[] EmissionPoints => _emissionPoints;

            private Microsoft.Xna.Framework.Vector3 _position = position;
            private readonly Vector3 _color = color.ToVector3().ToNumerics();

            public void GetLightInfo(Vector3 light, Vector3 hitPosition, out LightInfo lightInfo)
            {
                var lightPosition = light + Position;
                var toLight = lightPosition - hitPosition;
                var distance = toLight.Length();
                var toLightDir = Vector3.Normalize(toLight);
                lightInfo = new LightInfo(_color, toLightDir, distance);
            }
        }

        internal class SpotLight(
            Microsoft.Xna.Framework.Vector3 position,
            Microsoft.Xna.Framework.Vector3 direction,
            float innerAngleThresholdDeg,
            float outerAngleThresholdDeg,
            Microsoft.Xna.Framework.Color color
        ) : ILightSource
        {
            public Vector3 Position => _position.ToNumerics();
            public Vector3[] EmissionPoints => _emissionPoints;

            private Microsoft.Xna.Framework.Vector3 _position = position;
            private readonly Vector3 _color = color.ToVector3().ToNumerics();
            private readonly Vector3 _direction = Vector3.Normalize(direction.ToNumerics());
            private readonly float _innerAngleThresholdRad = float.DegreesToRadians(
                innerAngleThresholdDeg
            );
            private readonly float _outerAngleThresholdRad = float.DegreesToRadians(
                outerAngleThresholdDeg
            );

            public void GetLightInfo(Vector3 light, Vector3 hitPosition, out LightInfo lightInfo)
            {
                var lightPosition = light + Position;
                var toLight = lightPosition - hitPosition;
                var distance = toLight.Length();
                var toLightDir = Vector3.Normalize(toLight);
                var angle = float.Acos(Vector3.Dot(-toLightDir, _direction));

                // Calculate smooth angular attenuation
                var cosAngle = float.Cos(angle);
                var cosOuter = float.Cos(_outerAngleThresholdRad);
                var cosInner = float.Cos(_innerAngleThresholdRad);
                var attenuation = float.Clamp(
                    (cosAngle - cosOuter) / (cosInner - cosOuter),
                    0f,
                    1f
                );

                var attenuatedColor = _color * attenuation;
                lightInfo = new LightInfo(attenuatedColor, toLightDir, distance);
            }
        }
    }
}
