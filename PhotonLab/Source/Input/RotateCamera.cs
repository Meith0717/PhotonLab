// RotateCamera.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using MonoKit.Graphics.Camera;
using MonoKit.Input;

namespace PhotonLab.Source.Input;

public class RotateCamera(float rotationSpeed, Vector3 lookAtPos, float distance, float height) : ICamera3dBehavior
{
    private readonly float _rotateSpeed = rotationSpeed;
    private readonly  Vector3 _lookAtPos = lookAtPos;
    private readonly float  _distance = distance;
    private readonly float _height = height;
    
    public void Initialize(Camera3D owner)
    {
        owner.Position = new Vector3(0, _height, _distance);
        owner.Target = _lookAtPos;
    }

    public void Update(Camera3D owner, InputHandler inputHandler, double elapsedGameTime)
    {
        owner.Position = Vector3.Transform(owner.Position, Matrix.CreateRotationY(_rotateSpeed));
        owner.Target = _lookAtPos;
    }
}